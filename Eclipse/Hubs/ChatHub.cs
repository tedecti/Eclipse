using System.Security.Claims;
using Eclipse.Exceptions;
using Eclipse.Models;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace Eclipse.Hubs;

[Authorize]
[EnableCors("MyPolicy")]
public class ChatHub : Hub
{
    private readonly IChatRepository _chatRepository;
    private readonly ILogger<ChatHub> _logger;
    private const int MaxMessagesFetch = 50;
    private const int DefaultMessagesFetch = 20;

    public ChatHub(IChatRepository chatRepository, ILogger<ChatHub> logger)
    {
        _chatRepository = chatRepository;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = Context.User.FindFirst(ClaimTypes.NameIdentifier) ??
                          throw new UnauthorizedAccessException("User ID claim not found");
        return Guid.Parse(userIdClaim.Value);
    }

    private async Task NotifyOtherUserOfConnection(ChatRoom chatRoom, Guid currentUserId)
    {
        var otherUserId = chatRoom.UserId1 == currentUserId ? chatRoom.UserId2 : chatRoom.UserId1;
        await Clients.User(otherUserId.ToString()).SendAsync("UserConnected", new
        {
            UserId = currentUserId,
            RoomId = chatRoom.Id,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SendMessage(Guid chatRoomId, string messageText, string? replyId = null)
    {
        try
        {
            var senderId = GetCurrentUserId();
            var chatRoom = await _chatRepository.GetChatRoomAsync(chatRoomId);

            if (chatRoom == null)
                throw new NotFoundException("Room");

            var recipientId = chatRoom.UserId1 == senderId ? chatRoom.UserId2 : chatRoom.UserId1;

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ChatRoomId = chatRoomId,
                SenderId = senderId,
                MessageText = messageText,
                ReplyId = replyId,
                IsRead = false,
                Timestamp = DateTime.UtcNow
            };

            await _chatRepository.SaveMessageAsync(message);

            var messageDto = MessageDto.FromMessage(message);

            await Task.WhenAll(
                Clients.User(recipientId.ToString()).SendAsync("NewMessage", messageDto),
                Task.Run(() => _logger.LogInformation(
                    "Sent message {MessageId} in chat {ChatRoomId} from user {SenderId} to user {RecipientId}",
                    message.Id, chatRoomId, senderId, recipientId))
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message in chat room {ChatRoomId}", chatRoomId);
            throw new HubException("Failed to send message", ex);
        }
    }

    public async Task<IReadOnlyList<MessageDto>> GetChatHistory(Guid chatRoomId, int skip = 0,
        int take = DefaultMessagesFetch)
    {
        try
        {
            var userId = GetCurrentUserId();
            var chatRoom = await _chatRepository.GetChatRoomAsync(chatRoomId);

            if (chatRoom == null || (chatRoom.UserId1 != userId && chatRoom.UserId2 != userId))
                throw new UnauthorizedAccessException("User is not part of the chat room");

            take = Math.Min(take, MaxMessagesFetch);

            var messages = await _chatRepository.GetChatHistoryAsync(chatRoomId, skip, take);
            var messageDtos = messages.Select(MessageDto.FromMessage).ToList();

            _logger.LogInformation("Retrieved {MessageCount} messages for chat room {ChatRoomId}",
                messageDtos.Count, chatRoomId);

            return messageDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat history for room {ChatRoomId}", chatRoomId);
            throw new HubException($"Failed to get chat history: {ex.Message}");
        }
    }

    public async Task ReceiveMessage(Guid chatRoomId, Guid messageId)
    {
        var userId = Guid.Parse(Context.User.FindFirst("UserId").Value);

        var message = await _chatRepository.GetMessageAsync(messageId);
        if (message == null)
        {
            throw new ArgumentException("Message not found", nameof(messageId));
        }

        var chatRoom = await _chatRepository.GetChatRoomAsync(chatRoomId);
        if (chatRoom == null || (chatRoom.UserId1 != userId && chatRoom.UserId2 != userId))
        {
            throw new UnauthorizedAccessException("User is not part of the chat room.");
        }

        _logger.LogInformation("Message {MessageId} in chat room {ChatRoomId} was received by user {UserId}",
            messageId, chatRoomId, userId);

        await Clients.User(message.SenderId.ToString()).SendAsync("MessageReceived", new
        {
            ChatRoomId = chatRoomId,
            MessageId = messageId,
            ReceiverId = userId,
            Timestamp = DateTime.UtcNow
        });
    }


    public async Task MarkAsRead(Guid messageId)
    {
        try
        {
            var message = await _chatRepository.GetMessageAsync(messageId)
                          ?? throw new NotFoundException("Message");

            await Task.WhenAll(
                _chatRepository.MarkMessageAsReadAsync(messageId),
                Clients.User(message.SenderId.ToString()).SendAsync("MessageRead", messageId)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message {MessageId} as read", messageId);
            throw new HubException("Failed to mark message as read");
        }
    }

    public async Task AddReaction(Guid messageId, string reactionId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reactionId))
                throw new ArgumentException("Reaction ID cannot be empty", nameof(reactionId));

            var userId = GetCurrentUserId();
            var message = await _chatRepository.GetMessageAsync(messageId)
                          ?? throw new ArgumentException("Message not found", nameof(messageId));

            var chatRoom = await _chatRepository.GetChatRoomAsync(message.ChatRoomId);
            if (chatRoom == null || (chatRoom.UserId1 != userId && chatRoom.UserId2 != userId))
                throw new UnauthorizedAccessException("You are not authorized to add a reaction to this message");

            await _chatRepository.AddReactionAsync(messageId, reactionId);

            await Task.WhenAll(
                Clients.Users(new[] { message.SenderId.ToString(), userId.ToString() })
                    .SendAsync("ReactionAdded", new
                    {
                        MessageId = messageId,
                        ReactionId = reactionId,
                        UserId = userId,
                        Timestamp = DateTime.UtcNow
                    }),
                Task.Run(() => _logger.LogInformation(
                    "Reaction {ReactionId} added to message {MessageId} by user {UserId}",
                    reactionId, messageId, userId))
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding reaction to message {MessageId}", messageId);
            throw new HubException("Failed to add reaction", ex);
        }
    }

    public async Task PinMessage(Guid chatRoomId, Guid messageId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var chatRoom = await _chatRepository.GetChatRoomAsync(chatRoomId)
                           ?? throw new NotFoundException("Room");

            if (chatRoom.UserId1 != userId && chatRoom.UserId2 != userId)
                throw new UnauthorizedAccessException("You are not authorized to pin messages in this chat room");

            var message = await _chatRepository.GetMessageAsync(messageId);
            if (message == null || message.ChatRoomId != chatRoomId)
                throw new NotFoundException("Message");

            await _chatRepository.PinMessageAsync(chatRoomId, messageId);

            await Task.WhenAll(
                Clients.Users(new[] { chatRoom.UserId1.ToString(), chatRoom.UserId2.ToString() })
                    .SendAsync("MessagePinned", new
                    {
                        ChatRoomId = chatRoomId,
                        MessageId = messageId,
                        PinnedBy = userId,
                        Timestamp = DateTime.UtcNow
                    }),
                Task.Run(() => _logger.LogInformation(
                    "Message {MessageId} pinned in chat room {ChatRoomId} by user {UserId}",
                    messageId, chatRoomId, userId))
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pinning message {MessageId} in chat room {ChatRoomId}",
                messageId, chatRoomId);
            throw new HubException("Failed to pin message", ex);
        }
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        try
        {
            var currentUserId = GetCurrentUserId();
            var secondUserIdStr = Context.GetHttpContext()?.Request.Query["userId"].FirstOrDefault();

            if (!Guid.TryParse(secondUserIdStr, out Guid secondUserId))
                throw new ArgumentException("Invalid second user ID");

            var chatRoom = await _chatRepository.CreateOrGetChatRoomAsync(currentUserId, secondUserId);

            if (chatRoom == null)
                throw new InvalidOperationException("Could not create or retrieve chat room");

            await Task.WhenAll(
                Groups.AddToGroupAsync(connectionId, chatRoom.Id.ToString()),
                NotifyOtherUserOfConnection(chatRoom, currentUserId)
            );

            _logger.LogInformation("User {UserId} connected to chat room {ChatRoomId}", currentUserId, chatRoom.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection error for connection ID {ConnectionId}", connectionId);
            Context.Abort();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var currentUserId = GetCurrentUserId();
        var connectionId = Context.ConnectionId;

        try
        {
            var httpContext = Context.GetHttpContext();
            
            var secondUserIdStr = httpContext?.Request.Query["secondUserId"].FirstOrDefault();

            if (string.IsNullOrEmpty(secondUserIdStr) ||
                !Guid.TryParse(secondUserIdStr, out Guid secondUserId))
            {
                _logger.LogWarning("No valid second user ID found during disconnection");
                await base.OnDisconnectedAsync(exception);
                return;
            }

            var chatRoom = await _chatRepository.CreateOrGetChatRoomAsync(currentUserId, secondUserId);
            
            await Task.WhenAll(
                Groups.RemoveFromGroupAsync(connectionId, chatRoom.Id.ToString()),
                NotifyOtherUserOfDisconnection(chatRoom, currentUserId)
            );

            _logger.LogInformation("User {UserId} disconnected from chat room {ChatRoomId}",
                currentUserId, chatRoom.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error during disconnection for user {UserId} with connection ID {ConnectionId}",
                currentUserId, connectionId);
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }

    private async Task NotifyOtherUserOfDisconnection(ChatRoom chatRoom, Guid currentUserId)
    {
        var otherUserId = chatRoom.UserId1 == currentUserId ? chatRoom.UserId2 : chatRoom.UserId1;

        await Clients.User(otherUserId.ToString()).SendAsync("UserDisconnected", new
        {
            UserId = currentUserId,
            RoomId = chatRoom.Id,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task JoinChatRoom(string chatRoomId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(chatRoomId))
                throw new ArgumentException("ChatRoomId cannot be null or empty", nameof(chatRoomId));
            
            if (!Guid.TryParse(chatRoomId, out Guid parsedChatRoomId))
                throw new ArgumentException("Invalid chat room ID format", nameof(chatRoomId));

            var userId = GetCurrentUserId();
            var chatRoom = await _chatRepository.GetChatRoomAsync(parsedChatRoomId);
            
            if (chatRoom == null ||
                (chatRoom.UserId1 != userId && chatRoom.UserId2 != userId))
                throw new UnauthorizedAccessException("User is not authorized to join this chat room");
            
            await Task.WhenAll(
                Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId),
                Clients.Group(chatRoomId).SendAsync("UserJoined", new
                {
                    UserId = userId,
                    Context.ConnectionId,
                    ChatRoomId = chatRoomId,
                    Timestamp = DateTime.UtcNow
                }),
                Task.Run(() => _logger.LogInformation(
                    "User {UserId} joined chat room {ChatRoomId}", userId, chatRoomId))
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining chat room {ChatRoomId}", chatRoomId);
            throw new HubException("Failed to join chat room", ex);
        }
    }
}
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
    private const int MaxMessagesFetch = 50;
    private const int DefaultMessagesFetch = 20;

    public ChatHub(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    private Guid GetCurrentUserId()
    {
        var userId = Guid.Parse(Context.User.FindFirst("UserId").Value);
        return userId;
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
            
            string replyText = null;
            if (!string.IsNullOrEmpty(replyId))
            {
                var replyMessage = await _chatRepository.GetMessageAsync(Guid.Parse(replyId));
                replyText = replyMessage?.MessageText;
            }

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
            messageDto.ReplyText = replyText;

            await Clients.Group(chatRoomId.ToString()).SendAsync("NewMessage", messageDto);
        }
        catch (Exception ex)
        {
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

            return messageDtos;
        }
        catch (Exception ex)
        {
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
                    })
            );
        }
        catch (Exception ex)
        {
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

            await Clients.Users(new[] { chatRoom.UserId1.ToString(), chatRoom.UserId2.ToString() })
                    .SendAsync("MessagePinned", new
                    {
                        ChatRoomId = chatRoomId,
                        MessageId = messageId,
                        PinnedBy = userId,
                        Timestamp = DateTime.UtcNow
                    });
        }
        catch (Exception ex)
        {
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
            
            await Groups.AddToGroupAsync(connectionId, chatRoom.Id.ToString());
            
            await Groups.AddToGroupAsync(connectionId, $"user_{currentUserId}");
            
            var unreadMessages = await _chatRepository.GetChatHistoryAsync(chatRoom.Id, 0, MaxMessagesFetch);
            var unreadMessageDtos = unreadMessages
                .Where(m => !m.IsRead && m.SenderId != currentUserId)
                .Select(MessageDto.FromMessage)
                .ToList();

            if (unreadMessageDtos.Count != 0)
            {
                await Clients.Client(connectionId).SendAsync("LoadUnreadMessages", unreadMessageDtos);
            }

            await NotifyOtherUserOfConnection(chatRoom, currentUserId);
        }
        catch (Exception)
        {
            Context.Abort();
            throw;
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
                await base.OnDisconnectedAsync(exception);
                return;
            }

            var chatRoom = await _chatRepository.CreateOrGetChatRoomAsync(currentUserId, secondUserId);
            
            await Task.WhenAll(
                Groups.RemoveFromGroupAsync(connectionId, chatRoom.Id.ToString()),
                Groups.RemoveFromGroupAsync(connectionId, $"user_{currentUserId}"),
                NotifyOtherUserOfDisconnection(chatRoom, currentUserId)
            );
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
            
            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);
            
            var missedMessages = await _chatRepository.GetChatHistoryAsync(parsedChatRoomId, 0, MaxMessagesFetch);
            var missedMessageDtos = missedMessages
                .Where(m => !m.IsRead && m.SenderId != userId)
                .Select(MessageDto.FromMessage)
                .ToList();

            if (missedMessageDtos.Any())
            {
                await Clients.Caller.SendAsync("LoadMissedMessages", missedMessageDtos);
            }

            await Clients.Group(chatRoomId).SendAsync("UserJoined", new
            {
                UserId = userId,
                Context.ConnectionId,
                ChatRoomId = chatRoomId,
                Timestamp = DateTime.UtcNow
            });
            
        }
        catch (Exception ex)
        {
            throw new HubException("Failed to join chat room", ex);
        }
    }
}
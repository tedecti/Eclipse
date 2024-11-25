using Eclipse.Models;
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

    public ChatHub(IChatRepository chatRepository, ILogger<ChatHub> logger)
    {
        _chatRepository = chatRepository;
        _logger = logger;
    }

    public async Task SendMessage(Guid chatRoomId, string messageText, string? replyId = null)
    {
        var senderId = Guid.Parse(Context.User.FindFirst("UserId").Value);
        var chatRoom = await _chatRepository.GetChatRoomAsync(chatRoomId);
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

        await Clients.User(recipientId.ToString()).SendAsync("NewMessage", messageDto);

        _logger.LogInformation(
            "Sent message {MessageId} in chat {ChatRoomId} from user {SenderId} to user {RecipientId}",
            message.Id, chatRoomId, senderId, recipientId);
    }

    public async Task<List<MessageDto>> GetChatHistory(Guid chatRoomId)
    {
        try
        {
            _logger.LogInformation($"GetChatHistory called for room {chatRoomId}");

            var userId = Guid.Parse(Context.User.FindFirst("UserId").Value);
            var chatRoom = await _chatRepository.GetChatRoomAsync(chatRoomId);

            if (chatRoom == null || (chatRoom.UserId1 != userId && chatRoom.UserId2 != userId))
            {
                _logger.LogWarning($"Unauthorized access attempt to chat room {chatRoomId} by user {userId}");
                throw new HubException("User is not part of the chat room.");
            }

            var messages = await _chatRepository.GetChatHistoryAsync(chatRoomId, 0, 10);
            var messageDtos = messages.Select(MessageDto.FromMessage).ToList();

            _logger.LogInformation($"Retrieved {messageDtos.Count()} messages for chat room {chatRoomId}");

            return messageDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in GetChatHistory: {ex.Message}");
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

        // Уведомление отправителя, что сообщение было получено
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
        var message = await _chatRepository.GetMessageAsync(messageId);
        await _chatRepository.MarkMessageAsReadAsync(messageId);
        await Clients.User(message.SenderId.ToString()).SendAsync("MessageRead", messageId);
    }

    public async Task AddReaction(Guid messageId, string reactionId)
    {
        var userId = Guid.Parse(Context.User.FindFirst("UserId").Value);
        var message = await _chatRepository.GetMessageAsync(messageId);
        await _chatRepository.AddReactionAsync(messageId, reactionId);
        await Clients.Users(new[] { message.SenderId.ToString(), userId.ToString() })
            .SendAsync("ReactionAdded", messageId, reactionId);
    }

    public async Task PinMessage(Guid chatRoomId, Guid messageId)
    {
        var chatRoom = await _chatRepository.GetChatRoomAsync(chatRoomId);
        await _chatRepository.PinMessageAsync(chatRoomId, messageId);
        await Clients.Users(new[] { chatRoom.UserId1.ToString(), chatRoom.UserId2.ToString() })
            .SendAsync("MessagePinned", chatRoomId, messageId);
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var currentUserId = Guid.Parse(Context.User.FindFirst("UserId").Value);
            var secondUserIdStr = Context.GetHttpContext().Request.Query["userId"].ToString();
            if (string.IsNullOrEmpty(secondUserIdStr) || !Guid.TryParse(secondUserIdStr, out Guid secondUserId))
            {
                throw new ArgumentException("Invalid second user ID format");
            }

            var chatRoom = await _chatRepository.CreateOrGetChatRoomAsync(currentUserId, secondUserId);
            if (chatRoom == null)
            {
                throw new ArgumentException("Chat room not found for these users");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoom.Id.ToString());
            var otherUserId = chatRoom.UserId1 == currentUserId ? chatRoom.UserId2 : chatRoom.UserId1;
            await Clients.User(otherUserId.ToString()).SendAsync("UserConnected", new
            {
                UserId = currentUserId,
                RoomId = chatRoom.Id,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation($"User {currentUserId} connected to chat room {chatRoom.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during connection: {ex.Message}");
            Context.Abort();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var currentUserId = Guid.Parse(Context.User.FindFirst("UserId").Value);
            var secondUserIdStr = Context.GetHttpContext().Request.Query["secondUserId"].ToString();
            if (!string.IsNullOrEmpty(secondUserIdStr) && Guid.TryParse(secondUserIdStr, out Guid secondUserId))
            {
                var chatRoom = await _chatRepository.CreateOrGetChatRoomAsync(currentUserId, secondUserId);
                if (chatRoom != null)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoom.Id.ToString());
                    var otherUserId = chatRoom.UserId1 == currentUserId ? chatRoom.UserId2 : chatRoom.UserId1;
                    await Clients.User(otherUserId.ToString()).SendAsync("UserDisconnected", new
                    {
                        UserId = currentUserId,
                        RoomId = chatRoom.Id,
                        Timestamp = DateTime.UtcNow
                    });

                    _logger.LogInformation($"User {currentUserId} disconnected from chat room {chatRoom.Id}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during disconnection: {ex.Message}");
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }

    public async Task JoinChatRoom(string chatRoomId)
    {
        if (string.IsNullOrWhiteSpace(chatRoomId))
        {
            throw new ArgumentException("ChatRoomId cannot be null or empty", nameof(chatRoomId));
        }

        // Подключение текущего пользователя к группе
        await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);

        // Уведомление других участников комнаты о новом пользователе
        await Clients.Group(chatRoomId).SendAsync("UserJoined", Context.ConnectionId, chatRoomId);

        // Логирование
        Console.WriteLine($"User {Context.ConnectionId} joined chat room {chatRoomId}");
    }
}
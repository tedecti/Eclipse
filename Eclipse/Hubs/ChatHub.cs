using Eclipse.Models;
using Eclipse.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Eclipse.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatRepository _chatRepository;

    public ChatHub(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    public async Task SendMessage(Guid chatRoomId, string messageText, string? replyId = null)
    {
        var senderId = Guid.Parse(Context.User.FindFirst("UserId").Value);

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

        var chatRoom = await _chatRepository.GetChatRoomAsync(chatRoomId);
        var recipientId = chatRoom.UserId1 == senderId ? chatRoom.UserId2 : chatRoom.UserId1;
        await Clients.User(senderId.ToString()).SendAsync("MessageSent", message);
        await Clients.User(recipientId.ToString() ?? string.Empty).SendAsync("NewMessage", message);
    }

    public async Task MarkAsRead(Guid chatRoomId, Guid messageId)
    {
        await _chatRepository.MarkMessageAsReadAsync(messageId);
        var message = await _chatRepository.GetMessageAsync(messageId);
        await Clients.User(message.SenderId.ToString() ?? string.Empty).SendAsync("MessageRead", messageId);
    }

    public async Task AddReaction(Guid messageId, string reactionId)
    {
        var userId = Guid.Parse(Context.User.FindFirst("UserId").Value);
        await _chatRepository.AddReactionAsync(messageId, reactionId);

        var message = await _chatRepository.GetMessageAsync(messageId);
        await Clients.Users(new[] { message.SenderId.ToString(), userId.ToString() }!)
            .SendAsync("ReactionAdded", messageId, reactionId);
    }

    public async Task PinMessage(Guid chatRoomId, Guid messageId)
    {
        await _chatRepository.PinMessageAsync(chatRoomId, messageId);
        var chatRoom = await _chatRepository.GetChatRoomAsync(chatRoomId);

        await Clients.Users(new[] { chatRoom.UserId1.ToString(), chatRoom.UserId2.ToString() }!)
            .SendAsync("MessagePinned", chatRoomId, messageId);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
using Eclipse.Models;

namespace Eclipse.Repositories.Interfaces;

public interface IChatRepository
{
    Task<Message?> SaveMessageAsync(Message message);
    Task<ChatRoom?> GetChatRoomAsync(Guid chatRoomId);
    Task<Message?> GetMessageAsync(Guid messageId);
    Task MarkMessageAsReadAsync(Guid messageId);
    Task AddReactionAsync(Guid messageId, string reactionId);
    Task<string> PinMessageAsync(Guid chatRoomId, Guid messageId);
    Task<List<Message>> GetChatHistoryAsync(Guid chatRoomId, int skip, int take);
    Task<ChatRoom> CreateOrGetChatRoomAsync(Guid userId1, Guid userId2);
    Task<IEnumerable<ChatRoom>> GetUserChatRoomsAsync(Guid userId);
    Task<bool> ValidateChatRoomAccessAsync(Guid chatRoomId, Guid userId);
}
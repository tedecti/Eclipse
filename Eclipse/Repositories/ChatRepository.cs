using Eclipse.Data;
using Eclipse.Exceptions;
using Eclipse.Models;
using Eclipse.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eclipse.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Message?> SaveMessageAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<ChatRoom?> GetChatRoomAsync(Guid chatRoomId)
    {
        return await _context.ChatRooms
            .AsNoTracking()
            .Include(c => c.User1)
            .Include(c => c.User2)
            .FirstOrDefaultAsync(c => c.Id == chatRoomId);
    }

    public async Task<Message?> GetMessageAsync(Guid messageId)
    {
        return await _context.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == messageId);
    }

    public async Task MarkMessageAsReadAsync(Guid messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddReactionAsync(Guid messageId, string reactionId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null)
        {
            message.ReactionId = reactionId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> PinMessageAsync(Guid chatRoomId, Guid messageId)
    {
        var chatRoom = await _context.ChatRooms.FindAsync(chatRoomId)
                       ?? throw new NotFoundException("Room");

        var chatMessage = await GetMessageAsync(messageId)
                          ?? throw new NotFoundException("Message");

        chatRoom.PinnedMessageId = messageId;
        await _context.SaveChangesAsync();

        return chatMessage.MessageText 
               ?? throw new InvalidOperationException("Message text is null");
    }

    public async Task<List<Message>> GetChatHistoryAsync(Guid chatRoomId, int skip, int take)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.ChatRoomId == chatRoomId)
            .OrderByDescending(m => m.Timestamp)
            .Skip(skip)
            .Take(take)
            .Include(m => m.Sender)
            .ToListAsync();
    }

    public async Task<ChatRoom> CreateOrGetChatRoomAsync(Guid userId1, Guid userId2)
    {
        var existingChatRoom = await _context.ChatRooms
            .AsNoTracking()
            .FirstOrDefaultAsync(cr =>
                (cr.UserId1 == userId1 && cr.UserId2 == userId2) ||
                (cr.UserId1 == userId2 && cr.UserId2 == userId1));

        if (existingChatRoom != null)
        {
            return existingChatRoom;
        }
        
        var user1 = await _context.Users.FindAsync(userId1);
        var user2 = await _context.Users.FindAsync(userId2);

        if (user1 == null || user2 == null)
        {
            throw new NotFoundException("User 1 or User 2");
        }

        var newChatRoom = new ChatRoom
        {
            Id = Guid.NewGuid(),
            UserId1 = userId1,
            UserId2 = userId2,
            User1 = user1,
            User2 = user2,
        };

        _context.ChatRooms.Add(newChatRoom);
        await _context.SaveChangesAsync();

        return newChatRoom;
    }

    public async Task<IEnumerable<ChatRoom>> GetUserChatRoomsAsync(Guid userId)
    {
        return await _context.ChatRooms
            .Include(cr => cr.User1)
            .Include(cr => cr.User2)
            .Include(cr => cr.Messages.OrderByDescending(m => m.Timestamp).Take(1))
            .Where(cr => cr.UserId1 == userId || cr.UserId2 == userId)
            .ToListAsync();
    }


    public async Task<bool> ValidateChatRoomAccessAsync(Guid chatRoomId, Guid userId)
    {
        var chatRoom = await _context.ChatRooms
            .AsNoTracking()
            .Where(cr => cr.Id == chatRoomId)
            .Select(cr => new { cr.UserId1, cr.UserId2 })
            .FirstOrDefaultAsync();

        if (chatRoom == null)
            return false;

        return chatRoom.UserId1 == userId || chatRoom.UserId2 == userId;
    }
}
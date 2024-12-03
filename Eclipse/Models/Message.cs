namespace Eclipse.Models;

public class Message
{
    public Guid Id { get; set; }
    public Guid ChatRoomId { get; set; }
    public Guid SenderId { get; set; }
    public string MessageText { get; set; }
    public string? ReactionId { get; set; }
    public string? ReplyId { get; set; }
    public bool IsRead { get; set; }
    public DateTime Timestamp { get; set; }

    public User Sender { get; set; }
    public ChatRoom ChatRoom { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Eclipse.Models;

public class ChatRoom
{
    [Key] 
    public Guid Id { get; set; }

    [Required] 
    public Guid UserId1 { get; set; }

    [Required] 
    public Guid UserId2 { get; set; }

    public Guid? PinnedMessageId { get; set; }

    public User User1 { get; set; } = null!;
    public User User2 { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public Message? PinnedMessage { get; set; }
}
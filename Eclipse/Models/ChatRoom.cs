using System.ComponentModel.DataAnnotations;

namespace Eclipse.Models;

public class ChatRoom
{
    [Key] public Guid Id { get; set; }

    [Required] public Guid? UserId1 { get; set; }

    [Required] public Guid? UserId2 { get; set; }

    [Required] public Guid? MessageId { get; set; }

    public Guid? PinnedMessageId { get; set; }

    public User User1 { get; set; }
    public User User2 { get; set; }
    public IEnumerable<Message> Messages { get; set; }
    public Message PinnedMessage { get; set; }
}
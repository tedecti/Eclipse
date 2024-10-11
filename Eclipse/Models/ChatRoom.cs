using System.ComponentModel.DataAnnotations;

namespace Eclipse.Models;

public class ChatRoom
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string? UserId1 { get; set; }
    [Required]
    public string? UserId2 { get; set; }
    [Required]
    public string? MessageId { get; set; }
    public string? PinnedMessageId { get; set; }
    
    public User User1 { get; set; }
    public User User2 { get; set; }
    public IEnumerable<Message> Messages { get; set; }
    public Message PinnedMessage { get; set; }
}
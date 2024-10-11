using System.ComponentModel.DataAnnotations;

namespace Eclipse.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    [Required]
    [MaxLength(100)]
    public string Email { get; set; }
    [Required]
    [MinLength(8)]
    public string Password { get; set; }
    [Required]
    [MaxLength(50)]
    public string Username { get; set; }
    [MinLength(11)]
    public string Phone { get; set; }
    public string Pfp { get; set; }
    [DataType(DataType.DateTime)]
    public DateTime RegisteredAt { get; set; }
    public IEnumerable<ChatRoom> ChatRooms { get; set; }
    public IEnumerable<Message> Messages { get; set; }
    public IEnumerable<ConferenceMember> ConferenceMembers { get; set; }

}
using System.ComponentModel.DataAnnotations;

namespace Eclipse.Models;

public class User
{
    public ICollection<ChatRoom> ChatRooms = new List<ChatRoom>();
    public ICollection<ConferenceMember> ConferenceMembers = new List<ConferenceMember>();
    public ICollection<Contact> Contacts = new List<Contact>();
    public ICollection<Message> Messages = new List<Message>();

    [Key] public Guid Id { get; set; }

    [Required] [MaxLength(100)] public string Name { get; set; }

    [Required] [MaxLength(100)] public string Email { get; set; }

    [Required] public string Password { get; set; }

    [Required] public string Username { get; set; }

    public string Phone { get; set; }
    public string? Pfp { get; set; }
    public DateTime LastOnline { get; set; }
    [DataType(DataType.DateTime)] public DateTime RegisteredAt { get; set; }
}
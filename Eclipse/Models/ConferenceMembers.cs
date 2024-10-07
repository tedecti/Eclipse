namespace Eclipse.Models;

public class ConferenceMembers
{
    public Guid Id { get; set; }
    public string? ConferenceId { get; set; }
    public string? MemberId { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool? IsAdmin { get; set; }
    
    public Conference Conference { get; set; }
    public User Member { get; set; }
}
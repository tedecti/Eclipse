using System.ComponentModel.DataAnnotations;

namespace Eclipse.Models;

public class ConferenceMember
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string? ConferenceId { get; set; }
    [Required]
    public string? MemberId { get; set; }
    [Required]
    public DateTime JoinedAt { get; set; }
    [Required]
    public bool? IsAdmin { get; set; }
    
    public Conference Conference { get; set; }
    public User Member { get; set; }
}
namespace Eclipse.Models;

public class Conference
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public int MemberCount { get; set; }
    public string? ConferencePicture { get; set; }
    public IEnumerable<ConferenceMember> ConferenceMembers { get; set; }
}
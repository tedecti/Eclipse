namespace Eclipse.Models;

public class Contact
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid ContactUserId { get; set; }
    public User ContactUser { get; set; }
    public DateTime AddedDate { get; set; }
}
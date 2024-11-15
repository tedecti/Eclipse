namespace Eclipse.Models.Dto;

public class UserDtoForChats
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public string Pfp { get; set; }
    public DateTime LastOnline { get; set; }

    public UserDtoForChats(User user)
    {
        Id = user.Id;
        Name = user.Name;
        Username = user.Username;
        Pfp = user.Pfp;
        LastOnline = user.LastOnline;
    }
}
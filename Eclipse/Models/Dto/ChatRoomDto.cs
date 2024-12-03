namespace Eclipse.Models.Dto;

public class ChatRoomDto
{
    public Guid Id { get; set; }
    public UserDtoForChats OtherUser { get; set; }
    public MessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}
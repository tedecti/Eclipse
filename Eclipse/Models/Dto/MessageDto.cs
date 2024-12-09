namespace Eclipse.Models.Dto;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ChatRoomId { get; set; }
    public Guid SenderId { get; set; }
    public string MessageText { get; set; }
    public string ReplyId { get; set; }
    public string ReplyText { get; set; }
    public bool IsRead { get; set; }
    public string Timestamp { get; set; } // ISO 8601 string format

    public static MessageDto FromMessage(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            ChatRoomId = message.ChatRoomId,
            SenderId = message.SenderId,
            MessageText = message.MessageText,
            ReplyId = message.ReplyId,
            IsRead = message.IsRead,
            Timestamp = message.Timestamp.ToString("o") // ISO 8601 format
        };
    }
}
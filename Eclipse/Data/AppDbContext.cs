using Eclipse.Models;
using Microsoft.EntityFrameworkCore;

namespace Eclipse.Data;

public class AppDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public AppDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("Npgsql"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureHasManyWithOne<User, ChatRoom>(
            user => user.ChatRooms,
            chatRoom => chatRoom.User1,
            chatRoom => chatRoom.UserId1
        );

        modelBuilder.ConfigureHasManyWithOne<User, ChatRoom>(
            user => user.ChatRooms,
            chatRoom => chatRoom.User2,
            chatRoom => chatRoom.UserId2
        );

        modelBuilder.ConfigureHasManyWithOne<ChatRoom, Message>(
            chatRoom => chatRoom.Messages,
            message => message.ChatRoom,
            message => message.ChatRoomId
        );

        modelBuilder.ConfigureHasManyWithOne<User, Message>(
            user => user.Messages,
            message => message.Sender,
            message => message.SenderId
        );

        modelBuilder.ConfigureHasOneWithMany<Conference, ConferenceMember>(
            conferenceMember => conferenceMember.Conference,
            conference => conference.ConferenceMembers,
            conferenceMember => conferenceMember.ConferenceId
        );
        
        modelBuilder.ConfigureHasOneWithMany<User, ConferenceMember>(
            conferenceMember => conferenceMember.Member,
            user => user.ConferenceMembers,
            conferenceMember => conferenceMember.MemberId
        );
    }
}
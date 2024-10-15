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
        modelBuilder.Entity<ChatRoom>()
            .HasOne(c => c.User1)
            .WithMany()
            .HasForeignKey(c => c.UserId1)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChatRoom>()
            .HasOne(c => c.User2)
            .WithMany()
            .HasForeignKey(c => c.UserId2)
            .OnDelete(DeleteBehavior.Restrict);

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

    public DbSet<User> Users { get; set; } = default!;
    public DbSet<ChatRoom> ChatRooms { get; set; } = default!;
    public DbSet<Conference> Conferences { get; set; } = default!;
    public DbSet<ConferenceMember> ConferenceMembers { get; set; } = default!;
    public DbSet<Message> Messages { get; set; } = default!;

}
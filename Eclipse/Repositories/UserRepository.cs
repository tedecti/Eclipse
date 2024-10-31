using Eclipse.Data;
using Eclipse.Models;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eclipse.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserById(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        return user;
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user;
    }

    public async Task<List<User>> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return users;
    }

    public async Task<User?> UpdateUser(Guid userId, UserProfileDto userProfileDto)
    {
        var updatedUser = await _context.Users
            .Where(u => userId == u.Id)
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(u => u.Email, userProfileDto.Email)
                        .SetProperty(u => u.Phone, userProfileDto.Phone)
                        .SetProperty(u => u.Pfp, userProfileDto.Pfp)
                        .SetProperty(u => u.Name, userProfileDto.Name)
                        .SetProperty(u => u.Username, userProfileDto.Username));
        var user = await GetUserById(userId);
        return user;
    }

    public async Task<User> UploadAvatar(Guid userId, string fileName)
    {
        var user = await GetUserById(userId);
        user.Pfp = fileName;
        await _context.SaveChangesAsync();
        return user;
    }
}
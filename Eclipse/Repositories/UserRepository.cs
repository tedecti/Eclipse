using Eclipse.Data;
using Eclipse.Models;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Eclipse.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eclipse.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ICacheService _cacheService;
    private const string UserCacheKeyPrefix = "User_";
    private const string AllUsersCacheKey = "AllUsers";

    public UserRepository(AppDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    private static string GetUserCacheKey(Guid id) => $"{UserCacheKeyPrefix}{id}";
    private static string GetUserByEmailCacheKey(string email) => $"{UserCacheKeyPrefix}Email_{email}";
    
   
    public async Task<User?> GetUserById(Guid id)
    {
        var cacheKey = GetUserCacheKey(id);
        var cachedUser = _cacheService.Get<User>(cacheKey);
        
        if (cachedUser != null) return cachedUser;
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user != null)
        {
            _cacheService.Set(cacheKey, user);
        }
        
        return user;
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        var cacheKey = GetUserByEmailCacheKey(email);
        var cachedUser = _cacheService.Get<User>(cacheKey);
        
        if (cachedUser != null) return cachedUser;
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            _cacheService.Set(cacheKey, user);
            _cacheService.Set(GetUserCacheKey(user.Id), user);
        }
        return user;
    }


    public async Task<List<User>> GetAllUsers()
    {
        var cachedUsers = _cacheService.Get<List<User>>(AllUsersCacheKey);
        if (cachedUsers != null) return cachedUsers;
        
        var users = await _context.Users.ToListAsync();
        _cacheService.Set(AllUsersCacheKey, users);
        
        return users;
    }


    public async Task<User?> UpdateUser(Guid userId, UserProfileDto userProfileDto)
    {
        var updatedRows = await _context.Users
            .Where(u => userId == u.Id)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(u => u.Email, userProfileDto.Email)
                    .SetProperty(u => u.Phone, userProfileDto.Phone)
                    .SetProperty(u => u.Pfp, userProfileDto.Pfp)
                    .SetProperty(u => u.Name, userProfileDto.Name)
                    .SetProperty(u => u.Username, userProfileDto.Username));

        if (updatedRows != 1) return null;
        
        _cacheService.Remove(GetUserCacheKey(userId));
        _cacheService.Remove(AllUsersCacheKey);
        
        return await GetUserById(userId);
    }

    public async Task<User> UploadAvatar(Guid userId, string fileName)
    {
        var user = await GetUserById(userId);
        if (user == null) return null!;
        
        user.Pfp = fileName;
        await _context.SaveChangesAsync();
        
        _cacheService.Set(GetUserCacheKey(userId), user);
        _cacheService.Remove(AllUsersCacheKey);
        
        return user;
    }

    public async Task UpdateLastSeen(Guid userId)
    {
        var user = await GetUserById(userId);
        if (user != null)
        {
            await _context.Users.Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastOnline, DateTime.UtcNow));
            
            _cacheService.Remove(GetUserCacheKey(userId));
            _cacheService.Remove(AllUsersCacheKey);
        }
    }
}
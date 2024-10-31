using Eclipse.Models;
using Eclipse.Models.Dto;

namespace Eclipse.Repositories.Interfaces;

public interface IUserRepository
{
    public Task<User?> GetUserById(Guid id);
    public Task<User?> GetUserByEmail(string email);
    public Task<List<User>> GetAllUsers();
    public Task<User> UploadAvatar(Guid userId, string fileName);
    public Task<User?> UpdateUser(Guid userId, UserProfileDto userProfileDto);
}
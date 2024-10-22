using Eclipse.Models;

namespace Eclipse.Repositories.Interfaces;

public interface IUserRepository
{
    public Task<User?> GetUserById(Guid id);
    public Task<User?> GetUserByEmail(string email);
    public Task<List<User>> GetAllUsers();
}
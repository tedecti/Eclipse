using Eclipse.Models;

namespace Eclipse.Repositories.Interfaces;

public interface IUserRepository
{
    public Task<User?> GetUser(Guid id);
    public Task<List<User>> GetAllUsers();
}
using Eclipse.Models;

namespace Eclipse.Repositories.Interfaces;

public interface ISearchRepository
{
    Task<IEnumerable<User>> GetUserProfileInSearch(string query);
}
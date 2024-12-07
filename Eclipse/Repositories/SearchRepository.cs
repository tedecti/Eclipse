using Eclipse.Data;
using Eclipse.Models;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eclipse.Repositories;

public class SearchRepository : ISearchRepository
{
    private readonly AppDbContext _context;

    public SearchRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetUserProfileInSearch(string query, Guid currentUserId)
    {
        var lowercaseQuery = query.ToLower();

        var searchResult = await _context.Users
            .Where(u => 
                u.Id != currentUserId && // Exclude current user
                (EF.Functions.Like(u.Username.ToLower(), $"%{lowercaseQuery}%") ||
                 EF.Functions.Like(u.Email.ToLower(), $"%{lowercaseQuery}%") ||
                 EF.Functions.Like(u.Phone, $"%{query}%")))
            .ToListAsync();

        return searchResult;
    }
}
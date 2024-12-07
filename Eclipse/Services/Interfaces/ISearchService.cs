using Eclipse.Models.Dto;

namespace Eclipse.Services.Interfaces;

public interface ISearchService
{
    Task<List<UserProfileDto>> Search(string query);
}
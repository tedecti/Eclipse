using AutoMapper;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Eclipse.Services.Interfaces;

namespace Eclipse.Services;

public class SearchService : ISearchService
{
    private readonly ISearchRepository _searchRepository;
    private readonly IMapper _mapper;

    public SearchService(ISearchRepository searchRepository, IMapper mapper)
    {
        _searchRepository = searchRepository;
        _mapper = mapper;
    }

    public async Task<List<UserProfileDto>> Search(string query)
    {
        var result = await _searchRepository.GetUserProfileInSearch(query);
        return _mapper.Map<List<UserProfileDto>>(result);
    }
}
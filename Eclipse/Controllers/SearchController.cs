using Eclipse.Middlewares;
using Eclipse.Models.Dto;
using Eclipse.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Eclipse.Controllers;

[ApiController]
[Route("api")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("search")]
    public async Task<ApiResponse<List<UserProfileDto>>> Search([FromQuery] string query)
    {
        var response = await _searchService.Search(query);
        return new ApiResponse<List<UserProfileDto>> { Message = "Success", Data = response };
    }
}
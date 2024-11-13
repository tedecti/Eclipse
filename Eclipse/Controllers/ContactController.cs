using Eclipse.Middlewares;
using Eclipse.Models;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Eclipse.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eclipse.Controllers;

[Route("api/contact")]
[ApiController]
public class ContactController : ControllerBase
{
    private readonly IContactService _contactService;
    private readonly IContactRepository _contactRepository;
    private readonly string _successMessage = "Success";

    public ContactController(IContactService contactService, IContactRepository contactRepository)
    {
        _contactService = contactService;
        _contactRepository = contactRepository;
    }
    [Authorize]
    [HttpGet]
    public async Task<ApiResponse<List<ShortContactDto>?>> GetAllContacts([FromQuery] string? sortOrder = null)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (userIdClaim == null) throw new UnauthorizedAccessException();

        var userId = Guid.Parse(userIdClaim);

        List<ShortContactDto>? contacts;

        if (!string.IsNullOrWhiteSpace(sortOrder))
        {
            contacts = await _contactService.SortContactsByLastOnline(userId, sortOrder);
        }
        else
        {
            contacts = await _contactService.GetMappedListOfContacts(userId);
        }

        return new ApiResponse<List<ShortContactDto>?> { Message = "Success", Data = contacts };
    }

    [Authorize]
    [HttpPost]
    [Route("{contactId}/add")]
    public async Task<ApiResponse<ShortContactDto>> AddContact(Guid contactId)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim is { Value: null }) throw new UnauthorizedAccessException();

        var userId = Guid.Parse(userIdClaim.Value);
        var newContact = await _contactService.MapResponseOfNewContact(userId, contactId);
        return new ApiResponse<ShortContactDto>{ Message = _successMessage, Data = newContact };
    }

    [Authorize]
    [HttpDelete]
    [Route("{contactId}/remove")]
    public async Task<ApiResponse<object>> RemoveContact(Guid contactId)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim is { Value: null }) throw new UnauthorizedAccessException();

        var userId = Guid.Parse(userIdClaim.Value);
        await _contactRepository.RemoveFromContacts(userId, contactId);
        return new ApiResponse<object>{ Message = _successMessage, Data = null };
    }
}
using Eclipse.Models.Dto;

namespace Eclipse.Services.Interfaces;

public interface IContactService
{
    public Task<List<ShortContactDto>?> SortContactsByLastOnline(Guid userId, string sortOrder = "desc");
    public Task<List<ShortContactDto>?> GetMappedListOfContacts(Guid userId);
    public Task<ShortContactDto> MapResponseOfNewContact(Guid userId, Guid contactId);
}
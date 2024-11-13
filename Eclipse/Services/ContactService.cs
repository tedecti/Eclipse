using AutoMapper;
using Eclipse.Models;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Eclipse.Services.Interfaces;

namespace Eclipse.Services;

public class ContactService : IContactService
{
    private readonly IContactRepository _contactRepository;
    private readonly IMapper _mapper;

    public ContactService(IContactRepository contactRepository, IMapper mapper)
    {
        _contactRepository = contactRepository;
        _mapper = mapper;
    }

    public async Task<List<ShortContactDto>?> SortContactsByLastOnline(Guid userId, string sortOrder = "desc")
    {
        var contacts = await _contactRepository.AllContactsOfUser(userId);
        IEnumerable<Contact> sortedContactsQuery = sortOrder
            .Equals("asc", StringComparison.CurrentCultureIgnoreCase)
            ? contacts
                .OrderBy(c => c.User.LastOnline)
            : contacts
                .OrderByDescending(c => c.User.LastOnline);
        var sortedContacts = sortedContactsQuery.ToList();
        return _mapper.Map<List<ShortContactDto>>(sortedContacts);
    }


    public async Task<List<ShortContactDto>?> GetMappedListOfContacts(Guid userId)
    {
        var contacts = await _contactRepository.AllContactsOfUser(userId);
        return _mapper.Map<List<ShortContactDto>>(contacts);
    }

    public async Task<ShortContactDto> MapResponseOfNewContact(Guid userId, Guid contactId)
    {
        var newContact = await _contactRepository.AddToContacts(userId, contactId);
        return _mapper.Map<ShortContactDto>(newContact);
    }
}
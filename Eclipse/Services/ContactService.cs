using Eclipse.Models;
using Eclipse.Repositories.Interfaces;
using Eclipse.Services.Interfaces;

namespace Eclipse.Services;

public class ContactService : IContactService
{
    private readonly IContactRepository _contactRepository;

    public ContactService(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<List<Contact>?> SortContactsByLastOnline(Guid userId)
    {
        var contacts = await _contactRepository.AllContactsOfUser(userId);
        var sortedContacts = contacts.OrderByDescending(c => c.User.LastOnline).ToList();
        return sortedContacts;
    }
}
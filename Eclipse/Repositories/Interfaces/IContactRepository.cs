using Eclipse.Models;

namespace Eclipse.Repositories.Interfaces;

public interface IContactRepository
{
    public Task<List<Contact>> AllContactsOfUser(Guid userId);
    public Task<Contact> GetContactOfUserById(Guid userId, Guid contactId);
    public Task<int> GetContactsCount(Guid userId);
    public Task<bool> CheckIfContactExists(Guid userId, Guid contactId);
    public Task<Contact> AddToContacts(Guid userId, Guid contactId);
    public Task RemoveFromContacts(Guid userId, Guid contactId);
}
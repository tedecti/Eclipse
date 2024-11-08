using Eclipse.Data;
using Eclipse.Exceptions;
using Eclipse.Models;
using Eclipse.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eclipse.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly AppDbContext _context;
    private readonly IUserRepository _userRepository;

    public ContactRepository(AppDbContext context, IUserRepository userRepository)
    {
        _context = context;
        _userRepository = userRepository;
    }
    
    public async Task<List<Contact>> AllContactsOfUser(Guid userId)
    {
        var contacts = await _context.Contacts
            .Where(c => c.UserId == userId)
            .ToListAsync();
        if (contacts.Count == 0)
        {
            throw new NotFoundException("Contacts");
        }

        return contacts;
    }

    public async Task<Contact> GetContactOfUserById(Guid userId, Guid contactId)
    {
        var contact = await _context.Contacts
            .Where(c => c.UserId == userId && c.ContactUserId == contactId)
            .FirstOrDefaultAsync();
        if (contact == null) throw new NotFoundException("Contact");
        return contact;
    }
    
    public async Task<int> GetContactsCount(Guid userId)
    {
        return await _context.Contacts
            .CountAsync(c => c.UserId == userId);
    }
    
    public async Task<bool> CheckIfContactExists(Guid userId, Guid contactId)
    {
        return await _context.Contacts
            .AnyAsync(c => c.UserId == userId && c.ContactUserId == contactId);
    }
    
    public async Task<Contact> AddToContacts(Guid userId, Guid contactId)
    {
        var contact = await _userRepository.GetUserById(contactId);
        if (contact == null) throw new NotFoundException("Contact");
        var newContact = new Contact
        {
            UserId = userId,
            ContactUserId = contactId,
            AddedDate = DateTime.UtcNow
        };
        _context.Add(newContact);
        await _context.SaveChangesAsync();
        return newContact;
    }

    public async Task RemoveFromContacts(Guid userId, Guid contactId)
    {
        var contact = await _userRepository.GetUserById(contactId);
        if (contact == null) throw new NotFoundException("User");
        await _context.Contacts
            .Where(c => c.UserId == userId && c.ContactUserId == contactId)
            .ExecuteDeleteAsync();
    }

}
namespace Eclipse.Repositories.Interfaces;

public interface IFileRepository
{
    Task<string> SaveFile(IFormFile file);
    Task<Stream?> GetFile(string fileName);
    Task<bool> DeleteFileFromStorage(string fileName);
}
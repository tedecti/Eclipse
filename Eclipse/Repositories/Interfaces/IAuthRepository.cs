using Eclipse.Models;
using Eclipse.Models.Dto;

namespace Eclipse.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<User> Register(RegisterDto registerDto);
    Task<string?> Login(UserAuthDto userAuthDto);
    string GenerateJwtToken(User user);
}
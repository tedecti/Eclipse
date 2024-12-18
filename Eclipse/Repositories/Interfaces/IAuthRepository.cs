using Eclipse.Models;
using Eclipse.Models.Dto;

namespace Eclipse.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<User> Register(RegisterDto registerDto);
    Task<string?> Login(UserDto userDto);
    string GenerateJwtToken(User user);
}
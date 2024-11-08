using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eclipse.Data;
using Eclipse.Models;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Eclipse.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly ILogger<AuthRepository> _logger;

    public AuthRepository(AppDbContext context, IConfiguration configuration, ILogger<AuthRepository> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<User> Register(RegisterDto registerDto)
    {
        if (string.IsNullOrEmpty(registerDto.Email))
            throw new ArgumentNullException(registerDto.Email, "Email cannot be null or empty");
        if (string.IsNullOrEmpty(registerDto.Password))
            throw new ArgumentNullException(registerDto.Password, "Password cannot be null or empty");
        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            throw new InvalidOperationException("User with the same email already exists");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password, 12);
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = registerDto.Email,
            Username = registerDto.Username,
            Name = registerDto.Name,
            Password = hashedPassword,
            Phone = registerDto.Phone,
            RegisteredAt = DateTime.UtcNow,
            Pfp = ""
        };
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return newUser;
    }

    public async Task<string?> Login(UserDto userDto)
    {
        if (string.IsNullOrEmpty(userDto.Email) || string.IsNullOrEmpty(userDto.Password)) return null;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(userDto.Password, user.Password)) return null;

        return await Task.FromResult(GenerateJwtToken(user));
    }

    public string GenerateJwtToken(User user)
    {
        var jsonKey = _configuration.GetValue<string>("ApiSettings:Secret");
        if (string.IsNullOrEmpty(jsonKey))
        {
            _logger.LogError("SecretKey is missing or empty in configuration.");
            throw new InvalidOperationException("SecretKey is missing or empty in configuration.");
        }

        var key = Encoding.ASCII.GetBytes(jsonKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new("UserId", user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Username)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
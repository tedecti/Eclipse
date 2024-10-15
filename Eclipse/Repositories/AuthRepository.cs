using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eclipse.Data;
using Eclipse.Models;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace Eclipse.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthRepository> _logger;

    public AuthRepository(AppDbContext context, IConfiguration configuration, ILogger<AuthRepository> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<User> Register(RegisterDto registerDto)
    {
        if (string.IsNullOrEmpty(registerDto.Password) || string.IsNullOrEmpty(registerDto.Email))
        {
            throw new ArgumentException("Email and Password cannot be empty");
        }

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
        Console.WriteLine(newUser.Email);
        _context.Users.Add(newUser); 
        await _context.SaveChangesAsync();

        return newUser;
    }

    public async Task<string?> Login(UserDto userDto)
    {
        if (string.IsNullOrEmpty(userDto.Email) || string.IsNullOrEmpty(userDto.Password))
        {
            return null;
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(userDto.Password, user.Password))
        {
            return null;
        }

        return await Task.FromResult(GenerateJwtToken(user));
    }

    public string GenerateJwtToken(User user)
    {
        var jsonKey = _configuration.GetValue<string>("SecretKey");
        if (string.IsNullOrEmpty(jsonKey))
        {
            _logger.LogError("SecretKey is missing or empty in configuration.");
            throw new InvalidOperationException("SecretKey is missing or empty in configuration.");
        }

        var key = Encoding.ASCII.GetBytes(jsonKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
                { new Claim("id", user.Id.ToString()) }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
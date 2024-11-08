using Eclipse.Exceptions;
using Eclipse.Middlewares;
using Eclipse.Models;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Eclipse.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _authRepository;
    private readonly IUserRepository _userRepository;

    public AuthController(IAuthRepository authRepository, IUserRepository userRepository)
    {
        _authRepository = authRepository;
        _userRepository = userRepository;
    }

    [HttpPost]
    [Route("signup")]
    public async Task<ApiResponse<User>> Register([FromBody] RegisterDto registerDto)
    {
        var existUser = _userRepository.GetUserByEmail(registerDto.Email);
        if (existUser.Result != null) throw new AlreadyExistsException("User");
        var newUser = await _authRepository.Register(registerDto);
        return new ApiResponse<User> { Message = "Success", Data = newUser };
    }

    [HttpPost]
    [Route("signin")]
    public async Task<ApiResponse<object>> Login(UserDto userDto)
    {
        var token = await _authRepository.Login(userDto);
        if (token == null) throw new NotFoundException("User");
        return new ApiResponse<object> { Message = "Success", Data = new { token } };
    }
}
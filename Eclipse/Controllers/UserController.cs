using Eclipse.Exceptions;
using Eclipse.Middlewares;
using Eclipse.Models;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Eclipse.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eclipse.Controllers;

[Route("api")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IFileRepository _fileRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;
    private readonly string _successMessage = "Success";

    public UserController(IUserService userService, IFileRepository fileRepository, IUserRepository userRepository)
    {
        _userService = userService;
        _fileRepository = fileRepository;
        _userRepository = userRepository;
    }

    [Authorize]
    [HttpGet]
    [Route("me")]
    public async Task<ApiResponse<UserProfileDto>> GetMe()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim is { Value: null }) throw new UnauthorizedAccessException();

        var userId = Guid.Parse(userIdClaim.Value);
        var user = await _userService.GetUserMapped(userId);

        return new ApiResponse<UserProfileDto> { Message = _successMessage, Data = user };
    }

    [HttpGet]
    [Route("user/{userId:guid}")]
    public async Task<ApiResponse<UserProfileDto>> GetUserById(Guid userId)
    {
        var user = await _userService.GetUserMapped(userId);
        if (user == null) throw new NotFoundException("User");

        return new ApiResponse<UserProfileDto> { Message = _successMessage, Data = user };
    }

    [Authorize]
    [HttpPut]
    [Route("user/update/{userId:guid}")]
    public async Task<ApiResponse<User>> UpdateUserById(UserProfileDto userProfileDto)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim is { Value: null }) throw new UnauthorizedAccessException();

        var userId = Guid.Parse(userIdClaim.Value);
        var newUser = await _userRepository.UpdateUser(userId, userProfileDto);
        if (newUser == null) throw new NotFoundException("User");
        return new ApiResponse<User> { Message = _successMessage, Data = newUser };
    }

    [Authorize]
    [HttpPost("user/pfp/upload")]
    public async Task<ApiResponse<object>> UploadAvatar(IFormFile file)
    {
        var user = User.FindFirst("UserId")!.Value;
        if (user == null) throw new UnauthorizedAccessException();

        var userId = Guid.Parse(user);
        if (file == null) throw new NotFoundException("File");

        var fileName = await _fileRepository.SaveFile(file);
        await _userRepository.UploadAvatar(userId, fileName);
        return new ApiResponse<object> { Message = _successMessage, Data = fileName };
    }
}
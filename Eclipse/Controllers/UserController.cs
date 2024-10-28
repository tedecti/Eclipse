using System.Security.Claims;
using Eclipse.Exceptions;
using Eclipse.Middlewares;
using Eclipse.Models;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Eclipse.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Message = Microsoft.DotNet.Scaffolding.Shared.Messaging.Message;

namespace Eclipse.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IFileRepository _fileRepository;

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
            if (userIdClaim == null)
            {
                throw new NotFoundException("User");
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var user = await _userService.GetUserMapped(userId);

            return new ApiResponse<UserProfileDto> { Message = "Success", Data = user };
        }

        [HttpGet]
        [Route("user/{userId:guid}")]
        public async Task<ApiResponse<UserProfileDto>> GetUserById(Guid userId)
        {
            var user = await _userService.GetUserMapped(userId);
            if (user == null)
            {
                throw new NotFoundException("User");
            }

            return new ApiResponse<UserProfileDto> { Message = "Success", Data = user };
        }
        
        [Authorize]
        [HttpPost("/pfp/upload")]
        public async Task<ApiResponse<object>> UploadAvatar()
        {
            var httpRequest = HttpContext.Request;
            var file = httpRequest.Form.Files["image"];
            var user = User.FindFirst("UserId")!.Value;
            if (user == null)
            {
                throw new UnauthorizedAccessException();
            }

            var userId = Guid.Parse(user);
            if (file == null)
            {
                throw new NotFoundException("File");
            }

            var fileName = await _fileRepository.SaveFile(file);
            await _userRepository.UploadAvatar(userId, fileName);
            return new ApiResponse<object> { Message = "Success", Data = fileName };
        }
    }
}
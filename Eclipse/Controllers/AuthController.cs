using Eclipse.Models;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eclipse.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var newUser = await _authRepository.Register(registerDto);
            return Ok(newUser);
        }

        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> Login(UserDto userDto)
        {
            var user = await _authRepository.Login(userDto);
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(user);
        }
    }
}

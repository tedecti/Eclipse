using AutoMapper;
using Eclipse.Models.Dto;
using Eclipse.Repositories.Interfaces;
using Eclipse.Services.Interfaces;

namespace Eclipse.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;


    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserProfileDto> GetUserMapped(Guid userId)
    {
        var user = await _userRepository.GetUserById(userId);
        return _mapper.Map<UserProfileDto>(user);
    }
}
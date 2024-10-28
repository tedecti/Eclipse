using Eclipse.Models.Dto;

namespace Eclipse.Services.Interfaces;

public interface IUserService
{
    public Task<UserProfileDto> GetUserMapped(Guid userId);
}
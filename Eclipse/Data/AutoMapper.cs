using AutoMapper;
using Eclipse.Models;
using Eclipse.Models.Dto;

namespace Eclipse.Data;

public class AutoMapper : Profile
{
    public AutoMapper()
    {
        CreateMap<User, UserDto>();
        CreateMap<User, RegisterDto>();
        CreateMap<User, UserInfoDto>();
        CreateMap<User, UserProfileDto>();
        CreateMap<Contact, ShortContactDto>();
    }
}
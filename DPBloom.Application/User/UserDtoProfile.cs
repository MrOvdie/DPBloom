using AutoMapper;
using DPBloom.Application.Auth.Contracts;
using DPBloom.Core.User;

namespace DPBloom.Application.User;

public class UserDtoProfile :  Profile
{
    public UserDtoProfile()
    {
        CreateMap<UserModel, UserProfileDto>();
        CreateMap<RegisterUserDto, UserModel>();
    }
}
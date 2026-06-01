using AutoMapper;
using DPBloom.Application.Auth.Contracts;
using DPBloom.Application.User.Contracts;
using DPBloom.Core.User;

namespace DPBloom.Application.User;

public class UserDtoProfile : Profile
{
    public UserDtoProfile()
    {
        CreateMap<UserModel, UserProfileDto>();
        CreateMap<RegisterUserDto, UserModel>();
        CreateMap<UpdateUserProfileDto, UserModel>()
            .ForAllMembers(opt =>
                opt.Condition((_, _, srcMember) =>
                    srcMember is not null));
    
        CreateMap<UserModel, UserBaseInformationDto>();
    }
}
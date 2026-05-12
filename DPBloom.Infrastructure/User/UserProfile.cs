using AutoMapper;
using DPBloom.Core.User;

namespace DPBloom.Infrastructure.User;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<ApplicationUser, UserModel>().ReverseMap(); //TODO: check this
    }
}
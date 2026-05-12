using AutoMapper;
using DPBloom.Core.User;

namespace DPBloom.Infrastructure.User;

public class EnrollmentDaoProfile : Profile
{
    public EnrollmentDaoProfile()
    {
        CreateMap<UserEnrollmentModel, UserEnrollmentDao>()
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ReverseMap();
    }
}
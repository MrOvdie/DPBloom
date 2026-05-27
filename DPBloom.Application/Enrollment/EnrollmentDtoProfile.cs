using AutoMapper;
using DPBloom.Application.Enrollment.Contracts;
using DPBloom.Core.User;
using UUIDNext;

namespace DPBloom.Application.Enrollment;

public class EnrollmentDtoProfile : Profile
{
    public EnrollmentDtoProfile()
    {
        CreateMap<CreateEnrollment, UserEnrollmentModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));
        
        CreateMap<UpdateEnrollment, UserEnrollmentModel>()
            .ForAllMembers(opt => 
                opt.Condition((_, _, srcMember) => 
                    srcMember is not null));
    }
}
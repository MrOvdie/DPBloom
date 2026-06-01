using AutoMapper;
using DPBloom.Application.Enrollment.Contracts;
using DPBloom.Core.User;
using UUIDNext;

namespace DPBloom.Application.Enrollment;

public class EnrollmentDtoProfile : Profile
{
    public EnrollmentDtoProfile()
    {
        CreateMap<CreateEnrollmentDto, UserEnrollmentModel>();
        
        CreateMap<UpdateEnrollmentDto, UserEnrollmentModel>()
            .ForAllMembers(opt => 
                opt.Condition((_, _, srcMember) => 
                    srcMember is not null));
    }
}
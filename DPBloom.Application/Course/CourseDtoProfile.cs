using AutoMapper;
using DPBloom.Application.Course.Contracts;
using DPBloom.Core.Course;

namespace DPBloom.Application.Course;

public class CourseDtoProfile : Profile
{
    public CourseDtoProfile()
    {
        CreateMap<CourseDto, CourseModel>().ReverseMap();
        CreateMap<CreateCourse, CourseDto>();
        CreateMap<UpdateCourse, CourseDto>()
            .ForAllMembers(opt => 
                opt.Condition((_, _, srcMember) => 
                    srcMember != null));
    }
}
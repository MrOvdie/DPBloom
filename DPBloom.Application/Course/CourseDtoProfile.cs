using AutoMapper;
using DPBloom.Application.Course.Contracts;
using DPBloom.Core.Course;
using UUIDNext;

namespace DPBloom.Application.Course;

public class CourseDtoProfile : Profile
{
    public CourseDtoProfile()
    {
        CreateMap<CourseDto, CourseModel>().ReverseMap();
        CreateMap<CreateCourse, CourseModel>();
        CreateMap<UpdateCourse, CourseModel>()
            .ForAllMembers(opt =>
                opt.Condition((_, _, srcMember) =>
                    srcMember is not null));
    }
}
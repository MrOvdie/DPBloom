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
        CreateMap<CreateCourse, CourseModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsFinished, opt => opt.MapFrom(src => false));
        CreateMap<UpdateCourse, CourseModel>()
            .ForAllMembers(opt =>
                opt.Condition((_, _, srcMember) =>
                    srcMember is not null));
    } //TODO: Check if it working correctly
}
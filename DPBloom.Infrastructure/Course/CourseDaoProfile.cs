using AutoMapper;
using DPBloom.Core.Course;

namespace DPBloom.Infrastructure.Course;

public class CourseDaoProfile : Profile
{
    public CourseDaoProfile()
    {
        CreateMap<CourseDao, CourseModel>().ReverseMap();
    }
}
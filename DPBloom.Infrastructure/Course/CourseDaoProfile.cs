using AutoMapper;
using DPBloom.Application.Course.Contracts;
using DPBloom.Application.Exam;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Application.Lecture;
using DPBloom.Application.Lecture.Contracts;
using DPBloom.Application.Topic;
using DPBloom.Core.Course;
using DPBloom.Infrastructure.Exam;
using DPBloom.Infrastructure.Lecture;
using DPBloom.Infrastructure.Topic;

namespace DPBloom.Infrastructure.Course;

public class CourseDaoProfile : Profile
{
    public CourseDaoProfile()
    {
        CreateMap<CourseDao, CourseModel>().ReverseMap();
        CreateMap<CourseDao, CourseAggregateDto>();
        CreateMap<TopicDao, TopicDto>();
        CreateMap<LectureDao, LectureDto>();
        CreateMap<ExamDao, ExamDetailsDto>();
    }
}
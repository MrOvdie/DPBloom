using AutoMapper;
using DPBloom.Core.Exam;
using TestOfTesting.Models.Enums;

namespace DPBloom.Infrastructure.Exam;

public class ExamDaoProfile : Profile
{
    public ExamDaoProfile()
    {
        CreateMap<ExamDao, ExamModel>().ReverseMap();
        
        CreateMap<UserExamAttemptModel, UserExamAttemptDao>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

        CreateMap<UserExamAttemptDao, UserExamAttemptModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (AttemptStatus)src.Status));
    }
}
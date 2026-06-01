using AutoMapper;
using DPBloom.Application.Attempt.Contracts;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Core.Exam;

namespace DPBloom.Application.Attempt;

public class AttemptDtoProfile : Profile
{
    public AttemptDtoProfile()
    {
        
        CreateMap<SubmitAnswerDto, UserQuestionAnswerModel>();
        
        CreateMap<AttemptResultModel, AttemptResultDto>();

        CreateMap<UserExamAttemptAggregateModel, AttemptDetailsDto>()
            .ForMember(dest => dest.SavedAnswers, opt => opt.MapFrom(src => src.Answers));
        
        CreateMap<QuestionResultModel, QuestionResultDto>();

        
        CreateMap<UserExamAttemptModel, ExamAttemptDto>()
            .ForMember(dest => dest.FirstName, opt => opt.Ignore())
            .ForMember(dest => dest.MiddleName, opt => opt.Ignore())
            .ForMember(dest => dest.LastName, opt => opt.Ignore())
            .ForMember(dest => dest.UserGroup, opt => opt.Ignore());
    }
}
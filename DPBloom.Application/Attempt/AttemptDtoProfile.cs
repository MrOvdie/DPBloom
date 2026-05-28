using AutoMapper;
using DPBloom.Application.Attempt.Contracts;
using DPBloom.Core.Exam;

namespace DPBloom.Application.Attempt;

public class AttemptDtoProfile : Profile
{
    public AttemptDtoProfile()
    {
        CreateMap<SubmitAnswerDto, UserQuestionAnswerModel>();
        
    }
}
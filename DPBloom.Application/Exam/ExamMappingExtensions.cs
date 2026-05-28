using AutoMapper;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Core.Exam;

namespace DPBloom.Application.Exam;

public static class ExamMappingExtensions
{
    public static ExamDetailsDto ToDetailsDto(this ExamAggregateModel? aggregate, IMapper mapper)
    {
        if (aggregate is null) return null;

        var dto = mapper.Map<ExamDetailsDto>(aggregate.Exam); 
        
        dto.Questions = mapper.Map<List<QuestionDto>>(aggregate.Questions);
        
        foreach (var questionDto in dto.Questions)
        {
            var optionsForThisQuestion = aggregate.AnswerOptions
                .Where(o => o.QuestionId == questionDto.Id)
                .ToList();

            questionDto.Options = mapper.Map<List<OptionDto>>(optionsForThisQuestion);
        }
        
        return dto;
    }
}
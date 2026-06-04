using AutoMapper;
using DPBloom.Application.Attempt.Contracts;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Application.Exam.Contracts.Create;
using DPBloom.Application.Exam.Contracts.Update;
using DPBloom.Core.Exam;

namespace DPBloom.Application.Exam;

public class ExamDtoProfile : Profile
{
    public ExamDtoProfile()
    {
        CreateMap<ExamModel, ExamRecordDto>();
        CreateMap<ExamModel, ExamDetailsDto>();
        CreateMap<ExamModel, ExamDetailsForUpdateDto>();
        
        CreateMap<QuestionModel, QuestionDto>()
            .ForMember(dest => dest.Options, opt => opt.Ignore());
        
        CreateMap<QuestionModel, QuestionForUpdateDto>()
            .ForMember(dest => dest.Options, opt => opt.Ignore());

        CreateMap<AnswerOptionModel, OptionDto>();
        CreateMap<AnswerOptionModel, OptionForUpdateDto>();

        CreateMap<ExamAggregateModel, ExamDetailsDto>()
            .IncludeMembers(src => src.Exam)
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
            .AfterMap((src, dest, context) =>
            {
                if (src.AnswerOptions is not null)
                {
                    foreach (var question in dest.Questions)
                    {
                        question.Options = src.AnswerOptions
                            .Where(o => o.QuestionId == question.Id)
                            .Select(o => context.Mapper.Map<OptionDto>(o))
                            .ToList();
                    }
                }
            });
        
        CreateMap<ExamAggregateModel, ExamDetailsForUpdateDto>()
            .IncludeMembers(src => src.Exam)
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
            .AfterMap((src, dest, context) =>
            {
                if (src.AnswerOptions is not null)
                {
                    foreach (var question in dest.Questions)
                    {
                        question.Options = src.AnswerOptions
                            .Where(o => o.QuestionId == question.Id)
                            .Select(o => context.Mapper.Map<OptionForUpdateDto>(o))
                            .ToList();
                    }
                }
            });

        CreateMap<UserExamAttemptAggregateModel, AttemptDetailsDto>()
            .IncludeMembers(src => src.Attempt)
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
            .ForMember(dest => dest.SavedAnswers, opt => opt.MapFrom(src => src.Answers));

        CreateMap<UserExamAttemptModel, AttemptDetailsDto>();
        CreateMap<QuestionResultModel, QuestionResultDto>();
        CreateMap<UserQuestionAnswerModel, QuestionResultModel>();
        CreateMap<UserQuestionAnswerModel, SavedAnswerDto>();
        CreateMap<AttemptResultModel, AttemptResultRecordDto>();
        CreateMap<AttemptResultDto, AttemptResultModel>();
        CreateMap<QuestionResultDto, QuestionResultModel>();

        CreateMap<CreateExamDto, ExamModel>();
        CreateMap<UpdateExamDto, ExamModel>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CourseId, opt => opt.Ignore())
            .ForAllMembers(opt =>
                opt.Condition((_, _, srcMember) =>
                    srcMember is not null));

        CreateMap<CreateQuestionDto, QuestionModel>();
        CreateMap<UpdateQuestionDto, QuestionModel>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ExamId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore());

        CreateMap<CreateOptionDto, AnswerOptionModel>();
        CreateMap<UpdateOptionDto, AnswerOptionModel>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore());

        CreateMap<CreateExamDto, ExamAggregateModel>()
            .ForMember(dest => dest.Exam, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
            .AfterMap((src, dest, context) =>
            {
                dest.AnswerOptions = [];

                if (src.Questions is null || dest.Questions is null) return;

                var questionsList = dest.Questions.ToList();
                var srcQuestionsList = src.Questions.ToList();

                for (var i = 0; i < srcQuestionsList.Count; i++)
                {
                    var questionDto = srcQuestionsList[i];
                    var questionModel = questionsList[i];

                    questionModel.ExamId = dest.Exam.Id;

                    if (questionDto.Options is null) continue;

                    var options = context.Mapper.Map<List<AnswerOptionModel>>(questionDto.Options);

                    foreach (var option in options)
                    {
                        option.QuestionId = questionModel.Id;
                        dest.AnswerOptions.Add(option);
                    }
                }
            });

        CreateMap<UpdateExamDto, ExamAggregateModel>()
            .ForMember(dest => dest.Exam, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
            .AfterMap((src, dest, context) =>
            {
                dest.AnswerOptions ??= [];

                dest.AnswerOptions.Clear();

                if (src.Questions is null || dest.Questions is null) return;

                var questionsList = dest.Questions.ToList();
                var srcQuestionsList = src.Questions.ToList();

                for (var i = 0; i < srcQuestionsList.Count; i++)
                {
                    var questionDto = srcQuestionsList[i];
                    var questionModel = questionsList[i];

                    questionModel.ExamId = dest.Exam.Id;

                    if (questionDto.Options is null) continue;

                    var options = context.Mapper.Map<List<AnswerOptionModel>>(questionDto.Options);

                    foreach (var option in options)
                    {
                        option.QuestionId = questionModel.Id;
                        dest.AnswerOptions.Add(option);
                    }
                }
            });
    }
}
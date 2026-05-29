using AutoMapper;
using DPBloom.Application.Attempt.Contracts;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Core.Exam;

namespace DPBloom.Infrastructure.Exam;

public class ExamDaoProfile : Profile
{
    public ExamDaoProfile()
    {
        CreateMap<ExamModel, ExamDao>()
            .ForMember(dest => dest.Questions, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<UserExamAttemptModel, UserExamAttemptDao>().ReverseMap();
        CreateMap<UserExamAttemptDao, ExamAttemptDto>();
        CreateMap<UserQuestionAnswerModel, UserQuestionAnswerDao>().ReverseMap();

        CreateMap<QuestionModel, QuestionDao>()
            .ForMember(dest => dest.Exam, opt => opt.Ignore())
            .ForMember(dest => dest.Options, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<QuestionDao, QuestionDto>().ReverseMap();

        CreateMap<AnswerOptionModel, AnswerOptionDao>()
            .ForMember(dest => dest.Question, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<AnswerOptionDao, OptionDto>().ReverseMap();

        CreateMap<QuestionResultModel, QuestionResultDao>().ReverseMap(); //TODO: Check this later
        CreateMap<QuestionResultModel, QuestionResultDao>().ReverseMap(); 
        
        CreateMap<AttemptResultModel, AttemptResultDao>().ReverseMap();
        
        CreateMap<AttemptResultDao, AttemptResultDto>()
            .ForMember(dest => dest.StartedAt, opt => opt.MapFrom(src => src.Attempt.StartedAt))
            .ForMember(dest => dest.FinishedAt, opt => opt.MapFrom(src => src.Attempt.FinishedAt));
        
        CreateMap<UserExamAttemptDao, AttemptResultModel>().ReverseMap();

        CreateMap<ExamDao, ExamRecordDto>();

        //CreateMap<AnswerOptionDao, AnswerOptionModel>();
        /*CreateMap<ExamModel, ExamDao>().ReverseMap();

        CreateMap<UserExamAttemptModel, UserExamAttemptDao>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

        CreateMap<UserExamAttemptDao, UserExamAttemptModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (AttemptStatus)src.Status));

        CreateMap<UserQuestionAnswerModel, UserQuestionAnswerDao>().ReverseMap();

        CreateMap<QuestionModel, QuestionDao>()
            .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => (int)src.QuestionType))
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => (int)src.Level))
            .ForMember(dest => dest.CheckingType, opt => opt.MapFrom(src => (int)src.CheckingType));

        CreateMap<QuestionDao, QuestionModel>()
            .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => (QuestionType)src.QuestionType))
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => (Level)src.Level))
            .ForMember(dest => dest.CheckingType, opt => opt.MapFrom(src => (CheckingType)src.CheckingType));

        CreateMap<AnswerOptionModel, AnswerOptionDao>().ReverseMap();

        CreateMap<ExamDao, ExamAggregateModel>()
            .ForMember(dest => dest.Exam, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
            .ForMember(dest => dest.AnswerOptions, opt => opt.MapFrom(
                src => src.Questions.SelectMany(q => q.Options)));


        CreateMap<ExamAggregateModel, ExamDao>()
            .ForMember(dest => dest, opt => opt.MapFrom(src => src.Exam))
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src =>
                src.Questions.Select(q => q).ToList()));
        //src.AnswerOptions.Where(ao => ao.QuestionId.Equals(ao.Id)).ToList()));*/
    }
}
using AutoMapper;
using DPBloom.Core.Exam;
using DPBloom.Infrastructure.Data;
using Type = DPBloom.Core.Exam.Enums.Type;

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

        // From DAO to Domain
        // CreateMap<ExamDao, ExamModel>();

        CreateMap<QuestionModel, QuestionDao>()
            .ForMember(dest => dest.Exam, opt => opt.Ignore())
            .ForMember(dest => dest.Options, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<AnswerOptionModel, AnswerOptionDao>()
            .ForMember(dest => dest.Question, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<QuestionResultModel, QuestionResultDao>().ReverseMap(); //TODO: Check this later
        
        CreateMap<AttemptResultModel, AttemptResultDao>().ReverseMap();

        CreateMap<QuestionResultModel, QuestionResultDao>().ReverseMap(); 

        //CreateMap<AnswerOptionDao, AnswerOptionModel>();
        /*CreateMap<ExamModel, ExamDao>().ReverseMap();

        CreateMap<UserExamAttemptModel, UserExamAttemptDao>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

        CreateMap<UserExamAttemptDao, UserExamAttemptModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (AttemptStatus)src.Status));

        CreateMap<UserQuestionAnswerModel, UserQuestionAnswerDao>().ReverseMap();

        CreateMap<QuestionModel, QuestionDao>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => (int)src.Level))
            .ForMember(dest => dest.CheckingType, opt => opt.MapFrom(src => (int)src.CheckingType));

        CreateMap<QuestionDao, QuestionModel>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (Type)src.Type))
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
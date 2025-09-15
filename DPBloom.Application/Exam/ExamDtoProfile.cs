using AutoMapper;
using DPBloom.Core.Exam;
using TestOfTesting.Models.Enums;
using Type = TestOfTesting.Models.Enums.Type;

namespace DPBloom.Application.Exam;

public class ExamDtoProfile : Profile
{
    public ExamDtoProfile()
    {
            // ExamAggregateModel -> ExamDetailsDto
            CreateMap<ExamAggregateModel, ExamDetailsDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Exam.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Exam.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Exam.Description))
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.Exam.CourseId))
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.Exam.AuthorId))
                .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.Exam.TopicId))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Exam.Duration))
                .ForMember(dest => dest.StartsAt, opt => opt.MapFrom(src => src.Exam.StartsAt))
                .ForMember(dest => dest.FinishesAt, opt => opt.MapFrom(src => src.Exam.FinishesAt))
                .ForMember(dest => dest.CanSkip, opt => opt.MapFrom(src => src.Exam.CanSkip))
                .ForMember(dest => dest.ShowResults, opt => opt.MapFrom(src => src.Exam.ShowResults))
                .ForMember(dest => dest.IsRandomOrder, opt => opt.MapFrom(src => src.Exam.IsRandomOrder))
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

            // QuestionModel -> QuestionDto
            CreateMap<QuestionModel, QuestionDto>()
                .ForMember(dest => dest.Options, opt => opt.Ignore()); // Заповнимо окремо після мапінгу

            // OptionModel -> OptionDto
            CreateMap<AnswerOptionModel, OptionDto>();

            // Післямапінг для заповнення Options у QuestionDto
            CreateMap<ExamAggregateModel, ExamDetailsDto>()
                .AfterMap((src, dest, context) =>
                {
                    if (src.AnswerOptions is not null)
                    {
                        foreach (var question in dest.Questions)
                        {
                            var options = src.AnswerOptions
                                .Where(o => o.QuestionId == question.Id)
                                .Select(o => context.Mapper.Map<OptionDto>(o))
                                .ToList();

                            question.Options = options;
                        }
                    }
                });

            CreateMap<QuestionResultModel, QuestionResultDto>();
            /*//CreateMap<ExamModel, ExamDto>().ReverseMap();

            // CreateMap<UserExamAttemptModel, UserExamAttemptDto>()
            //     .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

            // CreateMap<UserExamAttemptDto, UserExamAttemptModel>()
            //     .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (AttemptStatus)src.Status));

            CreateMap<ExamAggregateModel, ExamDetailsDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Exam.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Exam.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Exam.Description))
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.Exam.CourseId))
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.Exam.AuthorId))
                .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.Exam.TopicId))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Exam.Duration))
                .ForMember(dest => dest.StartsAt, opt => opt.MapFrom(src => src.Exam.StartsAt))
                .ForMember(dest => dest.FinishesAt, opt => opt.MapFrom(src => src.Exam.FinishesAt))
                .ForMember(dest => dest.CanSkip, opt => opt.MapFrom(src => src.Exam.CanSkip))
                .ForMember(dest => dest.ShowResults, opt => opt.MapFrom(src => src.Exam.ShowResults))
                .ForMember(dest => dest.IsRandomOrder, opt => opt.MapFrom(src => src.Exam.IsRandomOrder))
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

            // Мапінг QuestionModel -> QuestionDto
            CreateMap<QuestionModel, QuestionDto>()
                .ForMember(dest => dest.Options, opt => opt.Ignore());

            // CreateMap<UserAnswerModel, UserAnswerDto>().ReverseMap();

            CreateMap<QuestionModel, QuestionDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => (int)src.Category))
                .ForMember(dest => dest.CheckingType, opt => opt.MapFrom(src => (int)src.CheckingType));

            CreateMap<QuestionDto, QuestionModel>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (Type)src.Type))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => (Category)src.Category))
                .ForMember(dest => dest.CheckingType, opt => opt.MapFrom(src => (CheckingType)src.CheckingType));

            CreateMap<AnswerOptionModel, OptionDto>().ReverseMap();*/
    }
}
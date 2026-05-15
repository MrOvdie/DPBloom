using AutoMapper;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Application.Exam.Contracts.Create;
using DPBloom.Core.Exam;
using UUIDNext;
using Type = DPBloom.Core.Exam.Enums.Type;

namespace DPBloom.Application.Exam;

public class ExamDtoProfile : Profile
{
    public ExamDtoProfile()
    {
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

            CreateMap<QuestionModel, QuestionDto>()
                .ForMember(dest => dest.Options, opt => opt.Ignore());

            CreateMap<AnswerOptionModel, OptionDto>();

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

            CreateMap<ExamModel, ExamRecordDto>();
            
            CreateMap<SubmitAnswerDto, UserQuestionAnswerModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.SubmittedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
           
            CreateMap<QuestionResultModel, QuestionResultDto>();
            
            CreateMap<AttemptResultModel, AttemptResultRecordDto>();
            
            CreateMap<AttemptResultDto, AttemptResultModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<QuestionResultDto, QuestionResultModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<CreateExamDto, ExamModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));
                
            CreateMap<CreateQuestionDto, QuestionModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));
                
            CreateMap<CreateOptionDto, AnswerOptionModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));

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
            
            // CreateMap<QuestionResultModel, QuestionResultDto>();
            //
            // CreateMap<AttemptResultDto, AttemptResultModel>()
            //     .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
            //     .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            //     .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));
            //
            // CreateMap<QuestionResultDto, QuestionResultModel>()
            //     .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
            //     .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            //     .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));
            //
            // CreateMap<CreateExamDto, ExamModel>()
            //     .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
            //     .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            //     .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));
            // CreateMap<CreateQuestionDto, QuestionModel>()
            //     .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
            //     .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            //     .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));
            // CreateMap<CreateOptionDto, AnswerOptionModel>()
            //     .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
            //     .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            //     .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));
            //
            // CreateMap<CreateExamDto, ExamAggregateModel>()
            //     // .ForMember(dest => dest.Exam.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
            //     // .ForMember(dest => dest.Exam.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            //     // .ForMember(dest => dest.Exam.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            //     .ForMember(dest => dest.Exam, opt => opt.MapFrom(src => src))
            //     .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
            //     .AfterMap((src, dest, context) =>
            //     {
            //         dest.AnswerOptions = [];
            //
            //         if (src.Questions is null) return;
            //         foreach (var questionDto in src.Questions)
            //         {
            //             if (questionDto.Options is null) continue;
            //             var options = context.Mapper.Map<List<AnswerOptionModel>>(questionDto.Options);
            //             dest.AnswerOptions.AddRange(options);
            //         }
            //     });
        
    }
}
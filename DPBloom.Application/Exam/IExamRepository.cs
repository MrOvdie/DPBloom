using DPBloom.Application.Base;
using DPBloom.Core.Exam;
using DPBloom.Infrastructure.Exam;

namespace DPBloom.Application.Exam;

public interface IExamRepository : IRepository<ExamModel>
{
    Task<ExamAggregateModel?> GetWithQuestionsAsync(Guid examId); //TODO: problems with aggregated model
    Task<List<UserAnswerModel>> GetUserAnswersForQuestionAsync(Guid attemptId, Guid questionId);
    Task<ExamAggregateModel> AddExamWithDetailsAsync(ExamAggregateModel model);
    Task<ExamAggregateModel> UpdateExamWithDetailsAsync(ExamAggregateModel model);
    Task DeleteExamWithDetailsAsync(ExamAggregateModel model);
    Task RestoreExamWithDetailsAsync(ExamAggregateModel model);
}
using DPBloom.Core.Exam;
using DPBloom.Infrastructure.Base;

namespace DPBloom.Infrastructure.Exam;

public interface IExamRepository : IRepository<ExamModel, ExamDao>
{
    Task<ExamAggregateModel> GetWithQuestionsAsync(Guid examId);
    Task<List<UserAnswerModel>> GetUserAnswersForQuestionAsync(Guid attemptId, Guid questionId);
    Task<ExamAggregateModel> AddExamWithDetailsAsync(ExamAggregateModel model);
    Task<ExamAggregateModel> UpdateExamWithDetailsAsync(ExamAggregateModel model);
    Task DeleteExamWithDetailsAsync(ExamAggregateModel model);
    Task RestoreExamWithDetailsAsync(ExamAggregateModel model);
}
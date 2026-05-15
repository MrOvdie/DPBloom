using DPBloom.Application.Base;
using DPBloom.Core.Exam;

namespace DPBloom.Application.Exam;

public interface IExamRepository : IRepository<ExamModel>
{
    Task<IReadOnlyList<ExamModel>> GetByCourseAsync(Guid courseId);
    Task<ExamAggregateModel?> GetWithQuestionsAsync(Guid examId); //TODO: problems with aggregated model
    Task<IReadOnlyList<UserQuestionAnswerModel>> GetUserAnswersForQuestionAsync(Guid attemptId, Guid questionId);
    Task<ExamAggregateModel> AddExamWithDetailsAsync(ExamAggregateModel model);
    Task<ExamAggregateModel> UpdateExamWithDetailsAsync(ExamAggregateModel model);
    Task DeleteExamWithDetailsAsync(ExamAggregateModel model);
    Task RestoreExamWithDetailsAsync(ExamAggregateModel model);
    Task<Guid?> GetCourseIdByExamIdAsync(Guid examId);
    Task<bool> IsExamAuthorAsync(Guid examId, Guid userId);
}
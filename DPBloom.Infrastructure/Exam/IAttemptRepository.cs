using DPBloom.Core.Exam;
using DPBloom.Infrastructure.Base;

namespace DPBloom.Infrastructure.Exam;

public interface IAttemptRepository : IRepository<UserExamAttemptModel, UserExamAttemptDao>
{
    Task<UserExamAttemptModel> GetWithAnswersAsync(Guid attemptId);
    Task<List<UserExamAttemptModel>> GetByUserAsync(Guid userId);

    Task<UserExamAttemptModel> StartAsync(UserExamAttemptModel model);
    Task SubmitAnswerAsync(Guid attemptId, UserAnswerModel answer);
    Task SaveAllAnswersAsync(Guid attemptId, List<UserAnswerModel> answers);
    Task FinishAsync(UserExamAttemptModel model);
    Task<QuestionResultModel> FindManuallyReviewedAnswer(Guid attemptId, Guid questionId);
}
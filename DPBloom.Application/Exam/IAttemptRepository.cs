using DPBloom.Application.Base;
using DPBloom.Core.Exam;
using DPBloom.Infrastructure.Exam;

namespace DPBloom.Application.Exam;

public interface IAttemptRepository : IRepository<UserExamAttemptModel>
{
    Task<UserExamAttemptModel> GetWithAnswersAsync(Guid attemptId);
    Task<List<UserExamAttemptModel>> GetByUserAsync(Guid userId);
   
    Task<UserExamAttemptModel> StartAsync(UserExamAttemptModel model);
    Task SubmitAnswerAsync(Guid attemptId, UserAnswerModel answer);
    Task SaveAllAnswersAsync(Guid attemptId, List<UserAnswerModel> answers);
    Task FinishAsync(UserExamAttemptModel model);
    Task<QuestionResultModel> FindManuallyReviewedAnswer(Guid attemptId, Guid questionId);
}
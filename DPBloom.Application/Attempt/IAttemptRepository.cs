using DPBloom.Application.Attempt.Contracts;
using DPBloom.Application.Base;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Core.Exam;

namespace DPBloom.Application.Attempt;

public interface IAttemptRepository : IRepository<UserExamAttemptModel>
{
    Task<UserExamAttemptModel> GetWithAnswersAsync(Guid attemptId);
    Task<List<UserExamAttemptModel>> GetByUserAsync(Guid userId);
    Task<List<UserExamAttemptModel>> GetByExamAsync(Guid examId);
   
    Task<UserExamAttemptModel> StartAsync(UserExamAttemptModel model);
    Task SubmitAnswerAsync(Guid attemptId, UserQuestionAnswerModel questionAnswer);
    Task SaveAllAnswersAsync(Guid attemptId, List<UserQuestionAnswerModel> answers);
    Task FinishAsync(UserExamAttemptModel model);
    Task<QuestionResultModel> FindManuallyReviewedAnswer(Guid attemptId, Guid questionId);
    Task<Guid?> GetCourseIdByAttemptIdAsync(Guid attemptId);
    Task<List<ActiveAttemptInfoDto>> GetActiveAttemptsInfoAsync(int batchSize);
    Task CloseAttemptsAsync(List<Guid> attemptIds);
    Task CloseAllExpiredAttemptsAsync();
}
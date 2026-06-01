using DPBloom.Application.Attempt.Contracts;
using DPBloom.Application.Base;
using DPBloom.Core.Exam;

namespace DPBloom.Application.Attempt;

public interface IAttemptRepository : IRepository<UserExamAttemptModel>
{
    Task<UserExamAttemptModel> GetWithAnswersAsync(Guid attemptId);
    Task<IReadOnlyList<UserExamAttemptModel>> GetByUserAsync(Guid userId);
    Task<IReadOnlyList<UserExamAttemptModel>> GetByExamAsync(Guid examId);

    Task<UserExamAttemptModel> StartAsync(UserExamAttemptModel model);
    /*Task SubmitAnswerAsync(Guid attemptId, UserQuestionAnswerModel questionAnswer);
    Task SaveAllAnswersAsync(Guid attemptId, List<UserQuestionAnswerModel> answers);*/
    Task FinishAsync(UserExamAttemptModel model);
    Task<QuestionResultModel> FindManuallyReviewedAnswer(Guid attemptId, Guid questionId);
    Task<Guid?> GetCourseIdByAttemptIdAsync(Guid attemptId);
    Task<IReadOnlyList<ActiveAttemptInfoDto>> GetActiveAttemptsInfoAsync(int batchSize);
    Task CloseAttemptsAsync(List<Guid> attemptIds);
    Task CloseAllExpiredAttemptsAsync();
    Task<IReadOnlyList<ExamAttemptDto>> GetAttemptsByUserByExamIdAsync(Guid userId, Guid examId);
    Task<UserExamAttemptModel?> GetAttemptWithFullDetailsAsync(Guid attemptId);
    Task<IReadOnlyList<UserQuestionAnswerModel>> GetAnswersByAttemptIdAsync(Guid attemptId);
    Task<Guid?> GetActiveAttemptIdAsync(Guid userId, Guid examId);
    Task<UserQuestionAnswerModel?> GetAnswerAsync(Guid attemptId, Guid questionId);
    Task AddAnswerAsync(UserQuestionAnswerModel questionAnswer);
    Task UpdateAnswerAsync(UserQuestionAnswerModel questionAnswer);
    Task SaveAnswersBatchAsync(List<UserQuestionAnswerModel> newAnswers,
        List<UserQuestionAnswerModel> existingAnswersToUpdate);

    Task<UserExamAttemptAggregateModel?> GetAttemptAggregateAsync(Guid attemptId);
    Task<int> GetAttemptCountAsync(Guid attemptId);
}
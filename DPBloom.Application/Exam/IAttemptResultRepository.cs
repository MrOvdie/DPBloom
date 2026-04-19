using DPBloom.Application.Base;
using DPBloom.Core.Exam;

namespace DPBloom.Application.Exam;

public interface IAttemptResultRepository : IRepository<AttemptResultModel>
{
    Task SaveManualQuestionAnswerReviewAsync(Guid attemptResultId, QuestionResultModel model);
    Task SaveAttemptResultAsync(Guid attemptId, AttemptResultModel model);
    Task<AttemptResultModel> GetAttemptResultByIdAsync(Guid attemptResultId);
    Task<AttemptResultModel> GetAttemptResultByExamIdAsync(Guid examId);
    Task<IReadOnlyList<AttemptResultModel>> GetAllAttemptsResultsByUserAsync(Guid userId);
    Task<IReadOnlyList<AttemptResultModel>> GetAllAttemptsResultsByExamAsync(Guid examId);

    //     Task<ExamAggregateModel> GetWithQuestionsAsync(Guid examId);
    // Task<List<UserAnswerModel>> GetUserAnswersForQuestionAsync(Guid attemptId, Guid questionId);
    // Task<ExamAggregateModel> AddExamWithDetailsAsync(ExamAggregateModel model);
    // Task<ExamAggregateModel> UpdateExamWithDetailsAsync(ExamAggregateModel model);
    // Task DeleteExamWithDetailsAsync(ExamAggregateModel model);
    // Task RestoreExamWithDetailsAsync(ExamAggregateModel model);

    // Task<UserExamAttemptModel> GetWithAnswersAsync(Guid attemptId);
    // Task<List<UserExamAttemptModel>> GetByUserAsync(Guid userId);
    //
    // Task<UserExamAttemptModel> StartAsync(UserExamAttemptModel model);
    // Task SubmitAnswerAsync(Guid attemptId, UserAnswerModel answer);
    // Task SaveAllAnswersAsync(Guid attemptId, List<UserAnswerModel> answers);
    // Task FinishAsync(UserExamAttemptModel model);
    // Task<QuestionResultModel> FindManuallyReviewedAnswer(Guid attemptId, Guid questionId);
}
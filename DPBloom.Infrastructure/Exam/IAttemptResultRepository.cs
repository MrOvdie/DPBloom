using DPBloom.Core.Exam;
using DPBloom.Infrastructure.Base;

namespace DPBloom.Infrastructure.Exam;

public interface IAttemptResultRepository : IRepository<AttemptResultModel, UserExamAttemptDao>
{
    Task SaveManualQuestionAnswerReviewAsync(Guid attemptId, Guid questionId, QuestionResultModel model);
    Task SaveAttemptResultAsync(Guid attemptId, AttemptResultModel model);
        
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
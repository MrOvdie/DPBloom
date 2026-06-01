using DPBloom.Application.Attempt.Contracts;
using DPBloom.Application.Base;
using DPBloom.Application.Exam;
using DPBloom.Core.Exam;

namespace DPBloom.Application.Attempt;

public interface IAttemptResultRepository : IRepository<AttemptResultModel>
{
    Task SaveManualQuestionAnswerReviewAsync(Guid attemptResultId, QuestionResultModel model);
    Task SaveAttemptResultAsync(Guid attemptId, AttemptResultModel model);
    Task<AttemptResultModel> GetAttemptResultByIdAsync(Guid attemptResultId);
    Task<AttemptResultModel> GetAttemptResultByExamIdAsync(Guid examId);
    Task<IReadOnlyList<AttemptResultModel>> GetAllAttemptsResultsByUserAsync(Guid userId);
    Task<IReadOnlyList<AttemptResultModel>> GetAllAttemptsResultsByUserByExamAsync(Guid userId, Guid examId);
    Task<IReadOnlyList<AttemptResultModel>> GetAllAttemptsResultsByExamAsync(Guid examId);
    Task<bool> IsAttemptResultOwnerAsync(Guid attemptResultId, Guid userId);
    Task<bool> IsExamAuthorByAttemptResultAsync(Guid attemptResultId, Guid userId);
    Task<List<UserQuestionAnswerModel>> GetAllManualReviewsForAttemptAsync(Guid attemptId);
    Task<List<AttemptResultModel>> GetAllManualReviewsAttemptsForExamAsync(Guid examId);
    Task<AttemptAccessInfo?> GetAccessInfoAsync(Guid attemptResultId);
    Task<AttemptResultRecordDto?> GetRecordByIdAsync(Guid attemptResultId);
    Task<AttemptResultModel> GetByIdWithDetailsAsync(Guid id);
}
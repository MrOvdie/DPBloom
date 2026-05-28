using DPBloom.Application.Attempt.Contracts;
using DPBloom.Application.Exam.Contracts;

namespace DPBloom.Application.Attempt;

public interface IAttemptService
{
    Task<Guid> StartAsync(Guid examId);
    Task SubmitAnswerAsync(Guid attemptId, SubmitAnswerDto dto);
    Task SaveAllAnswersAsync(Guid attemptId, List<SubmitAnswerDto> answers);
    Task<AttemptResultDto> FinishAsync(Guid attemptId);
    Task<AttemptResultDto> GetResultAsync(Guid attemptId);
    Task<AttemptResultRecordDto> GetResultRecordAsync(Guid attemptResultId);
    Task<IReadOnlyList<AttemptResultRecordDto>> GetAttemptResultsByExamAsync(Guid examId);

    Task<AttemptResultDto> CheckOpenTextAnswerAsync(Guid attemptResultId,
        IReadOnlyList<TeacherEvaluationDto> teacherEvaluations, Guid examId);

    Task<IReadOnlyList<AttemptResultRecordDto>> GetAttemptResultsForManualReviewByExamAsync(Guid examId);
    Task<IReadOnlyList<AttemptResultRecordDto>> GetUserExamAttempts(Guid userId, Guid examId);
}
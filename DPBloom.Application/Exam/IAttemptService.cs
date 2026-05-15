using DPBloom.Application.Base;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Core.Exam;

namespace DPBloom.Application.Exam;

public interface IAttemptService
{
    Task<Guid> StartAsync(Guid examId);
    Task SubmitAnswerAsync(Guid attemptId, SubmitAnswerDto dto);
    Task SaveAllAnswersAsync(Guid attemptId, List<SubmitAnswerDto> answers);
    Task<AttemptResultDto> FinishAsync(Guid attemptId);
    Task<AttemptResultDto> GetResultAsync(Guid attemptId);
    Task<IReadOnlyList<AttemptResultRecordDto>> GetAttemptResultsByExamAsync(Guid examId);

    Task<AttemptResultDto> CheckOpenTextAnswerAsync(Guid attemptResultId,
        IReadOnlyList<TeacherEvaluationDto> teacherEvaluations, Guid examId);

    Task<IReadOnlyList<AttemptResultRecordDto>> GetAttemptResultsForManualReviewByExamAsync(Guid examId);
}
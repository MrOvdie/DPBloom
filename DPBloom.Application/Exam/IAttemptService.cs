using DPBloom.Application.Base;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Core.Exam;

namespace DPBloom.Application.Exam;

public interface IAttemptService
{
    Task<Guid> StartAsync(Guid userId, Guid examId);
    Task SubmitAnswerAsync(Guid userId, Guid attemptId, SubmitAnswerDto dto);
    Task SaveAllAnswersAsync(Guid attemptId, List<SubmitAnswerDto> answers);
    Task<AttemptResultDto> FinishAsync(Guid userId, Guid attemptId);
    Task<AttemptResultDto> GetResultAsync(Guid userId, Guid attemptId);
    Task<UserExamAttemptModel> GetEntityByIdAsync(Guid id);
}
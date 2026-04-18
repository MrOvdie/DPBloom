using DPBloom.Application.Exam.Contracts;
using DPBloom.Core.Exam;

namespace DPBloom.Application.Exam;

public interface IAttemptService
{
    Task<Guid> StartAsync(Guid userId, Guid examId);
    Task SubmitAnswerAsync(/*Guid attemptId, */SubmitAnswerDto dto);
    Task SaveAllAnswersAsync(Guid attemptId, List<SubmitAnswerDto> answers);
    Task FinishAsync(Guid attemptId);
    Task<AttemptResultDto> GetResultAsync(Guid attemptId);
    Task<UserExamAttemptModel> GetEntityByIdAsync(Guid id);
}
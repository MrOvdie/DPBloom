using DPBloom.Application.Exam.Contracts;
using DPBloom.Application.Exam.Contracts.Create;
using DPBloom.Application.Exam.Contracts.Update;
using DPBloom.Core.Exam;

namespace DPBloom.Application.Exam;

public interface IExamService
{
    Task<IReadOnlyList<ExamRecordDto>> GetAllExamsAsync();
    Task<ExamRecordDto> GetExamOverviewByIdAsync(Guid examId);
    Task<ExamDetailsDto?> GetExamDetailsAsync(Guid examId);
    Task<IReadOnlyList<ExamRecordDto>> GetExamsByCourseAsync(Guid courseId);
    Task<Guid> CreateExamAsync(Guid courseId, CreateExamDto createExam);
    Task<ExamDetailsDto> UpdateExamAsync(Guid examId, UpdateExamDto updateExam);
    Task<ExamDetailsDto> DeleteExamAsync(Guid examId);
    Task<ExamDetailsDto> RestoreExamAsync(Guid examId);
    Task<ExamAggregateModel> GetEntityByIdAsync(Guid examId);
}
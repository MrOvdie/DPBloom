using DPBloom.Application.Exam.Contracts;
using DPBloom.Core.Exam;
using TestOfTesting.DTOs;

namespace DPBloom.Application.Exam;

public interface IExamService
{
    Task<ExamDetailsDto?> GetExamDetailsAsync(Guid id);
    Task<Guid> CreateExamAsync(CreateExamDto createExam);
    Task<ExamDetailsDto> UpdateExamAsync(Guid id, UpdateExamDto updateExam);
    Task<ExamDetailsDto> DeleteExamAsync(Guid id);
    Task<ExamDetailsDto> RestoreExamAsync(Guid id);
    Task<ExamAggregateModel> GetEntityByIdAsync(Guid id);
}
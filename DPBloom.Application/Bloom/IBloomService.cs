using DPBloom.Application.Bloom.Contracts;

namespace DPBloom.Application.Bloom;

public interface IBloomService
{
    // 1. Аналіз конкретної спроби (для студента)
    Task<BloomAnalysisDto> AnalyzeAndSaveAttemptAsync(Guid attemptResultId);
    Task<BloomAnalysisDto?> GetAnalysisByAttemptResultIdAsync(Guid attemptResultId);

    // 2. Агрегований аналіз курсу (для студента)
    Task<CourseBloomAnalysisDto> AnalyzeUserCourseAsync(Guid userId, Guid courseId);

    // 3. Глобальний профіль (для студента/куратора)
    Task<UserBloomProfileDto> GetUserOverallProfileAsync(Guid userId);

    // 4. Аналіз екзамену (для викладача)
    Task<ExamGroupAnalysisDto> GetExamAnalysisAsync(Guid examId);
}
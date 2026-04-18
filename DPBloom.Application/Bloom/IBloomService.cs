using DPBloom.Application.Bloom.Contracts;

namespace DPBloom.Application.Bloom;

public interface IBloomService
{
    Task<BloomAnalysisDto> AnalyzeAndSaveAttemptAsync(Guid attemptResultId);
    Task<BloomAnalysisDto> AnalyzeAndSaveCourseAsync(Guid attemptResultId);
    Task<BloomAnalysisDto?> GetAnalysisByAttemptResultIdAsync(Guid attemptResultId);
}
using DPBloom.Application.Bloom.Contracts;

namespace DPBloom.Application.Attempt.Contracts;

public class AttemptResultWithStatsDto
{
    public bool HasUncheckedAttempts { get; set; }
    
    public AttemptResultRecordDto AttemptResult { get; set; } = null!;

    public ExamAttemptDto? ExamAttempt { get; set; }
    
    public BloomAnalysisDto BloomAnalytics { get; set; } = null!;
}
using DPBloom.Application.Bloom.Contracts;

namespace DPBloom.Application.Attempt.Contracts;

public class AttemptResultWithStatsDto
{
    public AttemptResultRecordDto AttemptResult { get; set; } = null!; //TODO: add start and end times for the result to track the duration of the attempt
    
    public BloomAnalysisDto BloomAnalytics { get; set; } = null!;
}
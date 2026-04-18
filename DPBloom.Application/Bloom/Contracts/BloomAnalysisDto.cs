namespace DPBloom.Application.Bloom.Contracts;

public class BloomAnalysisDto
{
    public Guid Id { get; set; }
    public Guid AttemptResultId { get; set; }
    public string? OverallFeedback { get; set; }
    public List<BloomLevelPerformanceDto> PerformanceByLevel { get; set; }
    public List<RecommendedMaterialDto> Recommendations { get; set; }
}
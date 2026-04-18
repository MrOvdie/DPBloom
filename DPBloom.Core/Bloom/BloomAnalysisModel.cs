using DPBloom.Core.Base;

namespace DPBloom.Core.Bloom;

public class BloomAnalysisModel : EntityBase<Guid>
{
    public Guid AttemptResultId { get; set; }
    public Guid UserId { get; set; }
    public string? OverallFeedback { get; set; }
    public List<BloomLevelPerformance> PerformanceByLevel { get; set; }
    public List<RecommendedMaterial> Recommendations { get; set; }
}
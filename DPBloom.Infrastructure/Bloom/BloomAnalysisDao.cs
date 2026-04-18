using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Exam;
using DPBloom.Infrastructure.Extensions;

namespace DPBloom.Infrastructure.Bloom;

public class BloomAnalysisDao : EntityDaoBase<Guid>, ISoftDelete
{
    public Guid AttemptResultId { get; set; }
    public Guid UserId { get; set; }
    public string? OverallFeedback { get; set; }

    public virtual AttemptResultDao AttemptResult { get; set; }

    public virtual ICollection<BloomLevelPerformanceDao> PerformanceByLevel { get; set; }
    public virtual ICollection<RecommendedMaterialDao> Recommendations { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}
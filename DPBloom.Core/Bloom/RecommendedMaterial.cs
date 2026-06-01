using DPBloom.Core.Base;
using DPBloom.Core.Exam.Enums;

namespace DPBloom.Core.Bloom;

public class RecommendedMaterial : EntityBase<Guid>
{
    public Guid? MaterialId { get; set; }
    public Guid? TopicId { get; set; }
    public string? Title { get; set; }
    public string? AdviceText { get; set; }
    public BloomLevel? RelevantBloomLevel { get; set; }
}
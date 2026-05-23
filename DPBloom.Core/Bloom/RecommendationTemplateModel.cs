using DPBloom.Core.Base;
using DPBloom.Core.Exam.Enums;

namespace DPBloom.Core.Bloom;

public class RecommendationTemplateModel : EntityBase<Guid>
{
    public BloomLevel TargetLevel { get; set; } 
    
    public string AdviceText { get; set; } = string.Empty;
    
    public Guid? CourseId { get; set; } 
}
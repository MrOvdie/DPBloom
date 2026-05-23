using DPBloom.Core.Exam.Enums;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Extensions;

namespace DPBloom.Infrastructure.Bloom;

public class RecommendationTemplateDao : EntityDaoBase<Guid>, ISoftDelete
{
    public BloomLevel TargetLevel { get; set; } 
    
    public string AdviceText { get; set; } = string.Empty;
    
    public Guid? CourseId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}
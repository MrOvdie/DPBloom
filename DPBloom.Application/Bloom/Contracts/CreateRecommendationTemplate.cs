using DPBloom.Core.Exam.Enums;

namespace DPBloom.Application.Bloom.Contracts;

public class CreateRecommendationTemplate
{
    public BloomLevel TargetLevel { get; set; } 
    public string AdviceText { get; set; }
    public Guid? CourseId { get; set; }
}
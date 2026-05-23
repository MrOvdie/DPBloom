using DPBloom.Core.Exam.Enums;

namespace DPBloom.Application.Bloom.Contracts;

public class RecommendationTemplateDto
{
    public Guid Id { get; set; }
    public BloomLevel TargetLevel { get; set; } 
    public string AdviceText { get; set; } = string.Empty;
    public Guid? CourseId { get; set; }
}
using DPBloom.Core.Exam.Enums;

namespace DPBloom.Application.Bloom.Contracts;

public class RecommendationDto
{
    public BloomLevel RelevantBloomLevel { get; set; }
    
    public string AdviceText { get; set; } = string.Empty; 
    
    public Guid? RecommendedMaterialId { get; set; } 
    public string? RecommendedMaterialTitle { get; set; }
}
namespace DPBloom.Application.Bloom.Contracts;

public class CourseBloomAnalysisDto
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public List<BloomLevelPerformanceDto> AvaragePerformanceByLevel { get; set; }
    public List<RecommendedMaterialDto> CourseRecommendations { get; set; }   
}
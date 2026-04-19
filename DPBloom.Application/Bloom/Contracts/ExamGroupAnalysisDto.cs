namespace DPBloom.Application.Bloom.Contracts;

public class ExamGroupAnalysisDto
{
    public Guid ExamId { get; set; }
    public int TotalStudentsParticipated { get; set; }
    public List<BloomLevelGroupPerformanceDto> GroupPerformanceByLevel { get; set; }   
    public List<Guid> ProblematicTopicIds { get; set; }  
}
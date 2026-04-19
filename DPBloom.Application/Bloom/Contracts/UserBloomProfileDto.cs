namespace DPBloom.Application.Bloom.Contracts;

public class UserBloomProfileDto
{
    public Guid UserId { get; set; }
    public List<BloomLevelPerformanceDto> GlobalPerformanceByLevel { get; set; }
    public int TotalExamsAttempts { get; set; }
}
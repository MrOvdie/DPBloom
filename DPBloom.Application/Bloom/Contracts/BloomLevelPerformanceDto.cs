namespace DPBloom.Application.Bloom.Contracts;

public class BloomLevelPerformanceDto
{
    public string LevelName { get; set; } // "Knowing", "Applying" //TODO: maybe use int values, as everywhere, or simply enum
    public double ScorePercentage { get; set; }
    public bool IsWeakPoint { get; set; }
}
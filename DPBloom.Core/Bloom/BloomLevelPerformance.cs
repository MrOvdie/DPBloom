using TestOfTesting.Models.Enums;

namespace DPBloom.Core.Bloom;

public class BloomLevelPerformance
{
    public Category Level { get; set; }
    public double ScorePercentage { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public bool IsWeakPoint { get; set; }
}
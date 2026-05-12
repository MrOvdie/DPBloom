using DPBloom.Infrastructure.Base;

namespace DPBloom.Infrastructure.Bloom;

public class BloomLevelPerformanceDao : EntityDaoBase<Guid>
{
    public int Level { get; set; }
    public double ScorePercentage { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public bool IsWeakPoint { get; set; }

    public Guid BloomAnalysisId { get; set; }
    public virtual BloomAnalysisDao BloomAnalysis { get; set; }
}
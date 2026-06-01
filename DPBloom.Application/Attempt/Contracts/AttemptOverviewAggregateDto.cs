namespace DPBloom.Application.Attempt.Contracts;

public class AttemptOverviewAggregateDto
{
    public string ExamTitle { get; set; }
    public string? ExamDescription { get; set; }
    public AttemptResultDto AttemptResult { get; set; }
    public AttemptDetailsDto AttemptDetails { get; set; }
}
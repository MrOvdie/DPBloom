namespace DPBloom.Application.Exam.Contracts;

public class ActiveAttemptInfoDto
{
    public Guid AttemptId { get; set; }
    public DateTime StartedAt { get; set; }
    public TimeSpan Duration { get; set; }
}
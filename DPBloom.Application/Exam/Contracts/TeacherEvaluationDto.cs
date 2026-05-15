namespace DPBloom.Application.Exam.Contracts;

public class TeacherEvaluationDto
{
    public Guid QuestionId { get; set; }
    public double AwardedScore { get; set; }
    public string? Comment { get; set; }
}
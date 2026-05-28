using DPBloom.Application.Base;

namespace DPBloom.Application.Exam.Contracts.Update;

public class UpdateExamDto : ModelBase<Guid>
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? TopicId { get; set; }
    public TimeSpan? Duration { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? FinishesAt { get; set; }
    public double? MinimalPassScore { get; set; }
    public bool? CanSkip { get; set; }
    public bool? ShowResults { get; set; }
    public bool? CanCheckAttempts { get; set; }
    public bool? IsRandomOrder { get; set; }

    public List<UpdateQuestionDto>? Questions { get; set; }
}
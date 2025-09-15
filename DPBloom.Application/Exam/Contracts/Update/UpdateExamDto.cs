using DPBloom.Application.Base;

namespace DPBloom.Application.Exam.Contracts;

public class UpdateExamDto : ModelBase<Guid>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid CourseId { get; set; }
    public Guid AuthorId { get; set; }
    public TimeSpan Duration { get; set; }
    public bool CanSkip { get; set; }
    public bool ShowResults { get; set; }
    public bool IsRandomOrder { get; set; }

    public List<UpdateQuestionDto>? Questions { get; set; }
}
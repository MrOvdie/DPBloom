using DPBloom.Application.Base;

namespace DPBloom.Application.Course;

public class CourseDto : ModelBase<Guid>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid AuthorId { get; set; }
    public bool isFinished { get; set; }
    public bool isPublished { get; set; }
}
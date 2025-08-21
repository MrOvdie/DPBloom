using DPBloom.Application.Base;

namespace DPBloom.Application.Course;

public class CourseDto : ModelBase<Guid>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string AuthorId { get; set; }
}
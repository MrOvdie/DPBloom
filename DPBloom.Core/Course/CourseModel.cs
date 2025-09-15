using DPBloom.Core.Base;

namespace DPBloom.Core.Course;

public class CourseModel : EntityBase<Guid>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid AuthorId { get; set; }
}
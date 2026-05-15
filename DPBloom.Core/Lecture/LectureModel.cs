using DPBloom.Core.Base;

namespace DPBloom.Core.Lecture;

public class LectureModel : EntityBase<Guid>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid CourseId { get; set; }
    public Guid? TopicId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid LastUpdaterId { get; set; }
    public IEnumerable<string>? ContentLinks { get; set; }
    public IEnumerable<string>? FilePaths { get; set; }
}
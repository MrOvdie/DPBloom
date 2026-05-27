using DPBloom.Application.Base;

namespace DPBloom.Application.Topic;

public class TopicDto : ModelBase<Guid>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid AuthorId { get; set; }
    public Guid LastUpdaterId { get; set; }
    public Guid CourseId { get; set; }
}
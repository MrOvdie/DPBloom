using DPBloom.Core.Base;

namespace DPBloom.Core.Topic;

public class TopicModel : EntityBase<Guid>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid AuthorId { get; set; }
    public Guid CourseId { get; set; }
}
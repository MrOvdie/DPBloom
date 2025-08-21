using DPBloom.Application.Base;

namespace DPBloom.Application.Topic;

public class TopicDto : ModelBase<Guid>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string AuthorId { get; set; }
    public string CourseId { get; set; }
}
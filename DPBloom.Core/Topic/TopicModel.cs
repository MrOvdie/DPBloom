namespace DPBloom.Core.Topic;

public class TopicModel : EntityBase<Guid>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string AuthorId { get; set; }
    public string CourseId { get; set; }
}
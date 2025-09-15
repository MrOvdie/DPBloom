namespace DPBloom.Application.Topic.Contracts;

public class CreateTopic
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid AuthorId { get; set; }
    public Guid CourseId { get; set; }
}
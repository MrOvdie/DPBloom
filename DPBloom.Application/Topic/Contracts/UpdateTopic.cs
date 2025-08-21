namespace DPBloom.Application.Topic.Contracts;

public class UpdateTopic
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AuthorId { get; set; }
    public string? CourseId { get; set; }
}
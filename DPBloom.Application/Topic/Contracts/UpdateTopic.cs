namespace DPBloom.Application.Topic.Contracts;

public class UpdateTopic
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid? CourseId { get; set; }
}
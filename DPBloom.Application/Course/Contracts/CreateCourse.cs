namespace DPBloom.Application.Course.Contracts;

public class CreateCourse
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid AuthorId { get; set; }
}
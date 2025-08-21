namespace DPBloom.Application.Course.Contracts;

public class UpdateCourse
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string AuthorId { get; set; }
}
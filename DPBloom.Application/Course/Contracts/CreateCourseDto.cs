namespace DPBloom.Application.Course.Contracts;

public class CreateCourseDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public bool isPublished { get; set; }
}
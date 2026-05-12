namespace DPBloom.Application.Course.Contracts;

public class UpdateCourse
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool isFinished { get; set; }
    public bool isPublished { get; set; }
}
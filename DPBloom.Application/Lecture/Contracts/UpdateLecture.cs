namespace DPBloom.Application.Lecture.Contracts;

public class UpdateLecture
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? TopicId { get; set; }
    public Guid? AuthorId { get; set; }
    public IEnumerable<string>? ContentLinks { get; set; }
    public IEnumerable<string>? FilePaths { get; set; }
}
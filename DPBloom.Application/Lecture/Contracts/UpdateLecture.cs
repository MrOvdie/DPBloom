namespace DPBloom.Application.Lecture.Contracts;

public class UpdateLecture
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? CourseId { get; set; }
    public string? TopicId { get; set; }
    public string? AuthorId { get; set; }
    public IEnumerable<string>? ContentLinks { get; set; }
    public IEnumerable<string>? FilePaths { get; set; }
}
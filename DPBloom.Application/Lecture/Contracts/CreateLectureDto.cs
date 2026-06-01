namespace DPBloom.Application.Lecture.Contracts;

public class CreateLectureDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Content { get; set; }
    public Guid? TopicId { get; set; }
    public IEnumerable<string>? ContentLinks { get; set; }
    public IEnumerable<string>? FilePaths { get; set; }
}
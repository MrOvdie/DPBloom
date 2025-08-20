namespace DPBloom.Core.Lecture;

public class LectureModel : EntityBase<Guid>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string CourseId { get; set; }
    public string? TopicId { get; set; }
    public string AuthorId { get; set; }
    public string? ContentLink { get; set; }
    public string? FilePath { get; set; }
}
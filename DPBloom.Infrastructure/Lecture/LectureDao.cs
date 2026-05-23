using DPBloom.Core.Exam.Enums;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Course;
using DPBloom.Infrastructure.Extensions;
using DPBloom.Infrastructure.Topic;
using DPBloom.Infrastructure.User;

namespace DPBloom.Infrastructure.Lecture;

public class LectureDao : EntityDaoBase<Guid>, ISoftDelete
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Content { get; set; }
    public BloomLevel? TargetBloomLevel { get; set; }
    public Guid CourseId { get; set; }
    public Guid? TopicId { get; set; }
    public Guid AuthorId { get; set; }
    public IEnumerable<string>? ContentLinks { get; set; }
    public IEnumerable<string>? FilePaths { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    
    public CourseDao Course { get; set; }
    public TopicDao Topic { get; set; }
    public ApplicationUser Author { get; set; }
}
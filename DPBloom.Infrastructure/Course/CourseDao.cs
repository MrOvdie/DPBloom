using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Exam;
using DPBloom.Infrastructure.Extensions;
using DPBloom.Infrastructure.Lecture;
using DPBloom.Infrastructure.Topic;
using DPBloom.Infrastructure.User;

namespace DPBloom.Infrastructure.Course;

public class CourseDao : EntityDaoBase<Guid>, ISoftDelete
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid AuthorId { get; set; }
    public bool isFinished { get; set; }
    public bool isPublished { get; set; }

    public ApplicationUser Author { get; set; }
    public ICollection<TopicDao> Topics { get; set; } = new List<TopicDao>();
    public ICollection<LectureDao> Lectures { get; set; } = new List<LectureDao>();
    public ICollection<ExamDao> Exams { get; set; } = new List<ExamDao>();
    public ICollection<UserEnrollmentDao> Enrollments { get; set; } = new List<UserEnrollmentDao>();
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Course;
using DPBloom.Infrastructure.Extensions;
using DPBloom.Infrastructure.User;

namespace DPBloom.Infrastructure.Topic;

public class TopicDao : EntityDaoBase<Guid>, ISoftDelete
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid CourseId { get; set; }
    public Guid AuthorId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }

    public ApplicationUser Author { get; set; }
    public CourseDao Course { get; set; }
}
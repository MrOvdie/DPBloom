using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Course;
using DPBloom.Infrastructure.Extensions;

namespace DPBloom.Infrastructure.User;

public class UserEnrollmentDao : EntityDaoBase<Guid>, ISoftDelete
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public int Status { get; set; }
    public double ProgressPercentage { get; set; }
    public double FinalGrade { get; set; }

    public ApplicationUser User { get; set; }
    public CourseDao Course { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}
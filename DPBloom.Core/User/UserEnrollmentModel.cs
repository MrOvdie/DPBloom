using DPBloom.Core.Base;

namespace DPBloom.Core.User;

public class UserEnrollmentModel : EntityBase<Guid>
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public int Status { get; set; }
    public double ProgressPercentage { get; set; }
    public double FinalGrade { get; set; }
}
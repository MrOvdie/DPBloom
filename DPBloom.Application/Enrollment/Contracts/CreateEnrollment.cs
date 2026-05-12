using DPBloom.Core.User;

namespace DPBloom.Application.Enrollment.Contracts;

public class CreateEnrollment
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public EnrollmentStatusEnum Status { get; set; }
}
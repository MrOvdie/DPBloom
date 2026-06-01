using DPBloom.Core.User;

namespace DPBloom.Application.Enrollment.Contracts;

public class CreateEnrollmentDto
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public EnrollmentStatusEnum Status { get; set; }
}
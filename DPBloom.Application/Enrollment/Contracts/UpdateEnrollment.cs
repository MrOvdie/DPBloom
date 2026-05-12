using DPBloom.Core.User;

namespace DPBloom.Application.Enrollment.Contracts;

public class UpdateEnrollment
{
    public EnrollmentStatusEnum? Status { get; set; }
    public double? ProgressPercentage { get; set; }
    public double? FinalGrade { get; set; }
}
namespace DPBloom.Application.User.Contracts;

public class UpdateEnrollment
{
    public int? Status { get; set; }
    public double? ProgressPercentage { get; set; }
    public double? FinalGrade { get; set; }
}
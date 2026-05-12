namespace DPBloom.Application.User.Contracts;

public class UserEnrollmentDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public Guid CourseId { get; set; }
    public double ProgressPercentage { get; set; }
    public double FinalGrade { get; set; }
}
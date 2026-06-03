namespace DPBloom.Application.Auth.Contracts;

public class UpdateUserProfileDto
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Group { get; set; }
    public string? Faculty { get; set; }   
    public string? AvatarUrl { get; set; }

    public DateTime? EnteringDate { get; set; }
    public DateTime? GraduationDate { get; set; }
}
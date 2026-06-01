namespace DPBloom.Application.User.Contracts;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }
    
    public string Email { get; set; }
    public string? Group { get; set; }
    public string? Faculty { get; set; }
    public string? AvatarUrl { get; set; }
    
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();

    public List<string> Roles { get; set; }
    
    public DateTime FirstLogin { get; set; }
    public DateTime LastLogin { get; set; }
    public DateTime CreatedOn { get; set; }
    
    public DateTime? EnteringDate { get; set; }
    public DateTime? GraduationDate { get; set; }
}
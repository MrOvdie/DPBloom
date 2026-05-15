namespace DPBloom.Application.Auth.Contracts;

public class UpdateUserProfileDto
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Group { get; set; }
    public string? Faculty { get; set; }   
    public string? AvatarUrl { get; set; }  
}
namespace DPBloom.Application.Auth.Contracts;

public class RegisterUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; } 
    public string LastName { get; set; } = string.Empty;
    public string? Group { get; set; } 
    public string? Faculty { get; set; } 
}
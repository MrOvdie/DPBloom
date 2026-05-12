namespace DPBloom.Application.Auth.Contracts;

public class LoginRequestDto
{
    public string LoginDetails { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
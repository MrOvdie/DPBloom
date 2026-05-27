namespace DPBloom.Application.Auth.Contracts;

public class AuthResponseDto
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string FullName { get; set; }
    public string Username { get; set; }
    public string? AvatarUrl { get; set; }
}
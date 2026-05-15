using System.Security.Claims;
using DPBloom.Core.Base;

namespace DPBloom.Core.User;

public class UserModel : EntityBase<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    public string? Group { get; set; }
    public string? Faculty { get; set; }
    public string? AvatarUrl { get; set; }
    
    public IList<string> RoleClaims { get; set; }
    
    public DateTime FirstLogin { get; set; }
    public DateTime LastLogin { get; set; }
    
    public DateTime? EnteringDate { get; set; }
    public DateTime? GraduationDate { get; set; }
    
    public string GetFullName() => $"{FirstName} {MiddleName} {LastName}";
}
using Microsoft.AspNetCore.Identity;

namespace DPBloom.Infrastructure.User;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }
    public string? Group { get; set; }
    public string? Faculty { get; set; }
    public string? AvatarUrl { get; set; }

    public DateTime FirstLogin { get; set; }
    public DateTime LastLogin { get; set; }
    public DateTime CreatedOn { get; set; }

    public DateTime? DeletedOn { get; set; }
    public bool IsDeleted { get; set; }

    public DateTime? EnteringDate { get; set; }
    public DateTime? GraduationDate { get; set; }
}
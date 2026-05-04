using Microsoft.AspNetCore.Identity;

namespace DPBloom.Infrastructure.User;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; } //TODO: make it more detailed
}
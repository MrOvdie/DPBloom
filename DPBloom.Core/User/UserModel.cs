using DPBloom.Core.Base;

namespace DPBloom.Core.User;

public class UserModel : EntityBase<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    //TODO: Rework
    public string GetFullName() => $"{FirstName} {LastName}";
}
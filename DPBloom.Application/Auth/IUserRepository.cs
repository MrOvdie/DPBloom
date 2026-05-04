using System.Security.Claims;
using DPBloom.Core.User;

namespace DPBloom.Application.Auth;

public interface IUserRepository
{
    Task<UserModel?> ValidateUserAsync(string email, string password);
    Task<IReadOnlyList<Claim>> GetUserClaimsAsync(Guid userId);
    Task<bool> CreateUserAsync(UserModel user, string password, string initialRole);
}
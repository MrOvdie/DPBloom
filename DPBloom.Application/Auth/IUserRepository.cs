using System.Security.Claims;
using DPBloom.Core.User;

namespace DPBloom.Application.Auth;

public interface IUserRepository
{
    Task<UserModel?> ValidateUserAsync(string loginDetails, string password);
    Task<IReadOnlyList<Claim>> GetUserClaimsAsync(Guid userId);
    Task<bool> CreateUserAsync(UserModel user, string password, string initialRole);
    Task<bool> UpdateUserAsync(UserModel user);
    Task<UserModel?> GetUserByIdAsync(Guid userId);
    Task<IReadOnlyList<UserModel?>> GetUsersByIdsAsync(List<Guid> userIds);
    Task<UserModel?> GetUserByNameAsync(string userName);
    Task<IReadOnlyList<UserModel?>> GetUsersByGroupAsync(string group);

    Task<string> GetUserAccessTokens(Guid userId);
    Task RemoveRefreshTokenAsync(Guid userId);
}
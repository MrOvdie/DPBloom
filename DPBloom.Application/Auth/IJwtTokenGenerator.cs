using System.Security.Claims;
using DPBloom.Core.User;

namespace DPBloom.Application.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(UserModel user, IReadOnlyList<Claim> claims);
}
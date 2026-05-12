using DPBloom.Application.Auth.Contracts;
using DPBloom.Application.User;

namespace DPBloom.Application.Auth;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterUserDto userDto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
}
using DPBloom.Application.Auth.Contracts;
using DPBloom.Application.User;
using DPBloom.Application.User.Contracts;

namespace DPBloom.Application.Auth;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterUserDto userDto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task LogoutAsync();
    Task<UserProfileDto> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto dto);
    Task ChangeUserPasswordAsync(Guid userId, ChangePasswordDto dto);
}
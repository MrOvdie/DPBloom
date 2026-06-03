using DPBloom.Application.Auth;
using DPBloom.Application.Auth.Contracts;
using DPBloom.Application.User;
using DPBloom.Application.User.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPBloom.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto user)
    {
        var isRegistered = await _authService.RegisterAsync(user);

        if (!isRegistered)
        {
            return BadRequest(new
                { Message = "User with this email already exists or password is too weak." });
        }

        return Ok(new { Message = "Registration successful. Please log in." });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var responseDto = await _authService.LoginAsync(request);

        return Ok(responseDto);
    }
    
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
       await _authService.LogoutAsync();

        return Ok();
    }

    [HttpPut("update-profile/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> UpdateUserProfile(Guid userId, [FromBody] UpdateUserProfileDto updateProfileDto)
    {
        var result = await _authService.UpdateUserProfileAsync(userId, updateProfileDto);
       
        return Ok(result);
    }

    [HttpPut("change-password/{userId:guid}")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordDto changePasswordDto)
    {
        await _authService.ChangeUserPasswordAsync(userId, changePasswordDto);
        
        return Ok();
    }
}
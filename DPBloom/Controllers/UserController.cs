using System.Security.Claims;
using DPBloom.Application.Auth;
using DPBloom.Application.User;
using DPBloom.Application.User.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPBloom.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public UserController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var profile = await _authService.GetUserProfileAsync(userId);

        if (profile == null)
            return NotFound("Can't find user.");

        return Ok(profile);
    }
    
    [HttpGet("check-my-claims")]
    [Authorize(Policy = "RequireAdminPrivileges")]
    public IActionResult CheckClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        
        return Ok(claims);
    }

    [HttpGet("profile-statistics/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<GlobalUserDashboardDto>> GetUserStatistics(Guid userId)
    {
        var results = await _userService.GetGlobalUserDashboardStatisticsAsync(userId);
        
        return Ok(results);
    }
}
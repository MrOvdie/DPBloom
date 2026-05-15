using System.Security.Claims;
using DPBloom.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPBloom.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;

    public UserController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
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
}
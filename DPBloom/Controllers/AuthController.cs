// DPBloom/Controllers/AuthController.cs

using DPBloom.Application.Auth;
using DPBloom.Application.Auth.Contracts;
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
                { Message = "Користувач з таким email вже існує або пароль не відповідає вимогам безпеки." });
        }

        return Ok(new { Message = "Registration successful. Please log in." });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var responseDto = await _authService.LoginAsync(request);

        return Ok(responseDto);
    }
}
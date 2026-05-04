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
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var isRegistered = await _authService.RegisterAsync(
            request.Email, 
            request.Password, 
            request.FirstName, 
            request.LastName);

        if (!isRegistered)
        {
            return BadRequest(new { Message = "Користувач з таким email вже існує або пароль не відповідає вимогам безпеки." });
        }

        return Ok(new { Message = "Реєстрація пройшла успішно." });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var token = await _authService.LoginAsync(request.Email, request.Password);

        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized(new { Message = "Невірний email або пароль." });
        }

        return Ok(new AuthResponseDto { Token = token });
    }
}
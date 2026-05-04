// DPBloom/Controllers/UserController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPBloom.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Захищаємо всі ендпоінти цього контролера
public class UserController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        // Дістаємо ID користувача з Claims токена
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        // Дістаємо Email та Роль для демонстрації
        var email = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new 
        { 
            Id = userId, 
            Email = email, 
            Role = role 
        });
    }

    // Приклад використання специфічної політики доступу
    [HttpGet("admin-data")]
    [Authorize(Policy = "RequireAdminPrivileges")]
    public IActionResult GetAdminData()
    {
        return Ok(new { Message = "Це бачать тільки користувачі з правами адміністратора." });
    }
}
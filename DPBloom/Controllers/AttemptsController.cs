using System.Security.Claims;
using DPBloom.Application.Exam;
using DPBloom.Application.Exam.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPBloom.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttemptsController : ControllerBase
{
    private readonly IAttemptService _attemptService;

    public AttemptsController(IAttemptService attemptService)
    {
        _attemptService = attemptService;
    }

    // Почати нову спробу проходження екзамену
    [HttpPost("start/{examId:guid}")]
    public async Task<IActionResult> StartAttempt(Guid examId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        // Сервіс фіксує старт і повертає ID спроби
        var attemptId = await _attemptService.StartAsync(userId, examId);

        return Ok(new { AttemptId = attemptId });
    }

    // Відправити відповідь на конкретне запитання
    [HttpPost("{attemptId:guid}/submit")]
    public async Task<IActionResult> SubmitAnswer(Guid attemptId, [FromBody] SubmitAnswerDto request)
    {
        // Контролер витягує ID з токена (він це вміє і має право робити)
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        try
        {
            // Передаємо ID в сервіс
            await _attemptService.SubmitAnswerAsync(userId, attemptId, request);
            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            // Перехоплюємо виняток від сервісу і перетворюємо його на правильний HTTP статус (403 Forbidden)
            return Forbid(ex.Message); // або StatusCode(403, ex.Message)
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // Завершити спробу та отримати підсумковий результат
    [HttpPost("{attemptId:guid}/finish")]
    public async Task<IActionResult> FinishAttempt(Guid attemptId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var result = await _attemptService.FinishAsync(userId, attemptId);
        return Ok(result); // Повертає AttemptResultDto
    }

    // Отримати результати вже завершеної спроби (наприклад, для перегляду історії)
    [HttpGet("{attemptId:guid}/result")]
    public async Task<IActionResult> GetResult(Guid attemptId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var result = await _attemptService.GetResultAsync(userId, attemptId);
        if (result is null) return NotFound();

        return Ok(result);
    }
}
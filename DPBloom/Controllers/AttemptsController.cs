using System.Security.Claims;
using DPBloom.Application.Exam.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestOfTesting.Services.Interfaces;

namespace TestOfTesting.Controllers;

[ApiController]
[Route("api/attempts")]
[Authorize]
public class AttemptsController : ControllerBase
{
    private readonly IAttemptService _svc;
    public AttemptsController(IAttemptService svc) => _svc = svc;

    [HttpPost("start/{examId}")]
    public async Task<IActionResult> Start(int examId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var attemptId = await _svc.StartAsync(userId, examId);
        return Ok(attemptId);
    }

    [HttpPost("{attemptId}/answer")]
    public async Task<IActionResult> Submit(int attemptId, SubmitAnswerDto dto)
    {
        await _svc.SubmitAnswerAsync(attemptId, dto);
        return NoContent();
    }

    [HttpPost("{attemptId}/finish")]
    public async Task<IActionResult> Finish(int attemptId)
    {
        await _svc.FinishAsync(attemptId);
        return NoContent();
    }

    [HttpGet("{attemptId}/result")]
    public async Task<IActionResult> Result(int attemptId) =>
        Ok(await _svc.GetResultAsync(attemptId));
}
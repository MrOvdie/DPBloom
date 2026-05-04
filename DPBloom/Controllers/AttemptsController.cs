using System.Security.Claims;
using DPBloom.Application.Exam;
using DPBloom.Application.Exam.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPBloom.Controllers;

[ApiController]
[Route("api/attempts")]
[Authorize]
public class AttemptsController : ControllerBase
{
    private readonly IAttemptService _svc;
    public AttemptsController(IAttemptService svc) => _svc = svc;

    [HttpPost("start/{examId}")]
    public async Task<IActionResult> Start(Guid examId)
    {
        Guid userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var attemptId = await _svc.StartAsync(userId, examId);
        return Ok(attemptId);
    }

    [HttpPost("{attemptId}/answer")]
    public async Task<IActionResult> Submit(SubmitAnswerDto dto)
    {
        await _svc.SubmitAnswerAsync(dto);
        return NoContent();
    }

    [HttpPost("{attemptId}/finish")]
    public async Task<IActionResult> Finish(Guid attemptId)
    {
        await _svc.FinishAsync(attemptId);
        return NoContent();
    }

    [HttpGet("{attemptId}/result")]
    public async Task<IActionResult> Result(Guid attemptId) =>
        Ok(await _svc.GetResultAsync(attemptId));
}
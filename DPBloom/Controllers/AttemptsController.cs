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

    [HttpPost("{examId:guid}/start")]
    [Authorize]
    public async Task<IActionResult> StartAttempt(Guid examId)
    {
        var attemptId = await _attemptService.StartAsync(examId);

        return Ok(new { AttemptId = attemptId });
    }

    [HttpPost("{attemptId:guid}/submit")]
    [Authorize]
    public async Task<IActionResult> SubmitAnswer(Guid attemptId, [FromBody] SubmitAnswerDto request)
    {
        try
        {
            await _attemptService.SubmitAnswerAsync(attemptId, request);
            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{attemptId:guid}/submit-all")]
    [Authorize]
    public async Task<IActionResult> SubmitAnswers(Guid attemptId, [FromBody] List<SubmitAnswerDto> request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized(); //TODO: check

        try
        {
            await _attemptService.SaveAllAnswersAsync(attemptId, request);
            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{attemptId:guid}/finish")]
    [Authorize]
    public async Task<IActionResult> FinishAttempt(Guid attemptId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var result = await _attemptService.FinishAsync(attemptId);
        return Ok(result);
    }

    [HttpGet("{attemptId:guid}/result")]
    public async Task<IActionResult> GetResult(Guid attemptId)
    {
        var result = await _attemptService.GetResultAsync(attemptId);
        if (result is null) return NotFound();

        return Ok(result);
    }

    [HttpGet("{examId:guid}/exam-results")]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<IActionResult> GetAttemptResultsByExamAsync(Guid examId)
    {
        var result = await _attemptService.GetAttemptResultsByExamAsync(examId);
        
        return Ok(result);
    }

    [HttpPost("review/{attemptResultId:guid}")]
    [Authorize]
    public async Task<ActionResult<AttemptResultDto>> EvaluateManualAnswers(
        Guid attemptResultId,
        [FromBody] List<TeacherEvaluationDto> evaluations,
        [FromQuery] Guid examId)
    {
        var result = await _attemptService.CheckOpenTextAnswerAsync(attemptResultId, evaluations, examId);
        
        return Ok(result);
    }

    [HttpGet("{examId:guid}/manual-evaluations")]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<IActionResult> GetManualEvaluations(Guid examId)
    {
        var result = await _attemptService.GetAttemptResultsForManualReviewByExamAsync(examId);
        
        return Ok(result);
    }
}
using System.Security.Claims;
using DPBloom.Application.Attempt;
using DPBloom.Application.Attempt.Contracts;
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
    private readonly IAttemptAggregationService _attemptAggregationService;

    public AttemptsController(IAttemptService attemptService, IAttemptAggregationService attemptAggregationService)
    {
        _attemptService = attemptService;
        _attemptAggregationService = attemptAggregationService;
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

    [HttpGet("{attemptId:guid}/continue")]
    [Authorize]
    public async Task<ActionResult<AttemptDetailsDto>> ContinueAttempt(Guid attemptId)
    {
        var result = await _attemptService.ContinueAttemptAsync(attemptId);
        
        return Ok(result);
    }

    [HttpPost("{attemptId:guid}/finish")]
    [Authorize]
    public async Task<ActionResult<AttemptResultDto>> FinishAttempt(Guid attemptId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var result = await _attemptService.FinishAsync(attemptId);
        return Ok(result);
    }

    [HttpGet("{attemptId:guid}/result")]
    public async Task<ActionResult<AttemptResultDto>> GetAttemptResultWithStats(Guid attemptId)
    {
       var result =  await _attemptService.GetResultAsync(attemptId);
       
       return Ok(result);
    }

    [HttpGet("{examId:guid}/attempts/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<AttemptResultRecordDto>>> GetAttemptResultsByExamAndUserAsync(
        Guid examId, Guid userId)
    {
        var result = await _attemptService.GetUserExamResultsAttempts(userId, examId);
        
        return Ok(result);
    }

    [HttpGet("{examId:guid}/attempts-stats/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<AttemptResultWithStatsDto>>> GetAttemptsStatsByExamAndUserAsync(Guid examId, Guid userId)
    {
        var attemptAggregate = await _attemptAggregationService.GetAttemptResultsWithStatisticsByExamByUserAsync(userId, examId);
        
        return Ok(attemptAggregate);
    }
    
    [HttpGet("/attempt-stats/{attemptResultId:guid}")]
    [Authorize]
    public async Task<ActionResult<AttemptResultWithStatsDto>> GetAttemptStatsByExamAndUserAsync(Guid attemptResultId)
    {
        var attemptAggregate = await _attemptAggregationService.GetAttemptResultsWithStatisticsByIdAsync(attemptResultId);
        
        return Ok(attemptAggregate);
    }

    [HttpGet("{examId:guid}/exam-results")]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<ActionResult<IReadOnlyList<AttemptResultRecordDto>>> GetAttemptResultsByExamAsync(Guid examId)
    {
        var result = await _attemptService.GetAttemptResultsByExamAsync(examId);

        return Ok(result);
    }

    [HttpPost("review/{attemptResultId:guid}")]
    [Authorize]
    public async Task<ActionResult<AttemptResultDto>> EvaluateManualAnswers(
        Guid attemptResultId,
        [FromBody] List<TeacherEvaluationDto> evaluations)
    {
        var result = await _attemptService.CheckOpenTextAnswerAsync(attemptResultId, evaluations);

        return Ok(result);
    }

    [HttpGet("{examId:guid}/manual-evaluations")]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<ActionResult<List<AttemptResultRecordDto>>> GetManualEvaluations(Guid examId)
    {
        var result = await _attemptService.GetAttemptResultsForManualReviewByExamAsync(examId);

        return Ok(result);
    }
}
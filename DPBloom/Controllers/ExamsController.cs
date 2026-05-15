using DPBloom.Application.ANN;
using DPBloom.Application.Exam;
using DPBloom.Application.Exam.Contracts.Create;
using DPBloom.Application.Exam.Contracts.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPBloom.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExamsController : ControllerBase
{
    private readonly IExamService _examService;
    private readonly IBloomLevelPredictor _bloomPredictor;

    public ExamsController(IExamService examService, IBloomLevelPredictor bloomPredictor)
    {
        _examService = examService;
        _bloomPredictor = bloomPredictor;
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var exams = await _examService.GetAllExamsAsync();
       
        return Ok(exams);
    }

    [HttpGet("course/{courseId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetExamsByCourse(Guid courseId)
    {
        var exams = await _examService.GetExamsByCourseAsync(courseId);
        
        return Ok(exams);
    }
    
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var exam = await _examService.GetExamDetailsAsync(id);
        if (exam == null) return NotFound();
        
        return Ok(exam);
    }

    [HttpPost("{courseId:guid}")]
    [Authorize(Policy = "RequireTeacherPrivileges")] 
    public async Task<IActionResult> Create(Guid courseId, [FromBody] CreateExamDto request)
    {
        var examId = await _examService.CreateExamAsync(courseId, request);
        return CreatedAtAction(nameof(GetById), new { id = examId }, request);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireTeacherPrivileges")] 
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExamDto request)
    {
        await _examService.UpdateExamAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireTeacherPrivileges")] 
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _examService.DeleteExamAsync(id);
        return Ok(result);
    }
    
    [HttpPatch("{id:guid}/restore")]
    [Authorize(Policy = "RequireTeacherPrivileges")]  
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _examService.RestoreExamAsync(id);
        return Ok(result);
    }

    [HttpPost("predict-bloom")]
    [Authorize(Policy = "RequireTeacherPrivileges")] 
    public async Task<IActionResult> PredictBloomLevel([FromBody] string questionText)
    {
        var level = await _bloomPredictor.PredictLevelAsync(questionText);
        return Ok(new { predictedLevel = level });
    }
}
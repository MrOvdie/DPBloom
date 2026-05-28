using DPBloom.Application.ANN;
using DPBloom.Application.Exam;
using DPBloom.Application.Exam.Contracts;
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
    public async Task<ActionResult<List<ExamRecordDto>>> GetAll()
    {
        var exams = await _examService.GetAllExamsAsync();
       
        return Ok(exams);
    }

    [HttpGet("course/{courseId:guid}")]
    [Authorize]
    public async Task<ActionResult<List<ExamRecordDto>>> GetExamsByCourse(Guid courseId)
    {
        var exams = await _examService.GetExamsByCourseAsync(courseId);
        
        return Ok(exams);
    }
    
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<ActionResult<ExamDetailsDto>> GetByWithDetailsId(Guid id)
    {
        var exam = await _examService.GetExamDetailsAsync(id);
        if (exam == null) return NotFound();
        
        return Ok(exam);
    }
    
    //TODO: write a method for exam preview for the exam page

    [HttpGet("overview/{examId:guid}")]
    [Authorize]
    public async Task<ActionResult<ExamRecordDto>> GetOverviewById(Guid examId)
    {
        var result = await _examService.GetExamOverviewByIdAsync(examId);
        
        return Ok(result);
    }
    
    [HttpPost("{courseId:guid}")]
    [Authorize(Policy = "RequireTeacherPrivileges")] 
    public async Task<ActionResult<ExamDetailsDto>> Create(Guid courseId, [FromBody] CreateExamDto request)
    {
        var examId = await _examService.CreateExamAsync(courseId, request);
        
        return CreatedAtAction(nameof(GetByWithDetailsId), new { id = examId }, request);
    }

    [HttpPut("{examId:guid}")]
    [Authorize(Policy = "RequireTeacherPrivileges")] 
    public async Task<IActionResult> Update(Guid examId, [FromBody] UpdateExamDto request)
    {
        var result = await _examService.UpdateExamAsync(examId, request);
        
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireTeacherPrivileges")] 
    public async Task<ActionResult<ExamDetailsDto>> Delete(Guid id)
    {
        var result = await _examService.DeleteExamAsync(id);
        
        return Ok(result);
    }
    
    [HttpPatch("{id:guid}/restore")]
    [Authorize(Policy = "RequireTeacherPrivileges")]  
    public async Task<ActionResult<ExamDetailsDto>> Restore(Guid id)
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
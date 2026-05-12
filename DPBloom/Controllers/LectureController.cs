using DPBloom.Application.Lecture;
using DPBloom.Application.Lecture.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPBloom.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LectureController : ControllerBase
{
    private readonly ILectureService _lectureService;

    public LectureController(ILectureService lectureService)
    {
        _lectureService = lectureService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var lectures = await _lectureService.GetAllAsync();
        return Ok(lectures);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var lecture = await _lectureService.GetByIdAsync(id);
        if (lecture is null) return NotFound();

        return Ok(lecture);
    }

    [HttpPost("{courseId:guid}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<IActionResult> Create(Guid courseId, [FromBody] CreateLecture request)
    {
        var lectureDto = await _lectureService.CreateAsync(courseId, request);
        return CreatedAtAction(nameof(GetById), new { id = lectureDto.Id }, lectureDto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLecture request)
    {
        var result = await _lectureService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _lectureService.DeleteAsync(id);
        return Ok(result);
    }
    
    [HttpPatch("{id:guid}/restore")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _lectureService.RestoreAsync(id);
        return Ok(result);
    }
}
using System.Security.Claims;
using DPBloom.Application.Course;
using DPBloom.Application.Course.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPBloom.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await _courseService.GetAllAsync();
        return Ok(courses);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var course = await _courseService.GetByIdAsync(id);
        if (course is null) return NotFound();

        return Ok(course);
    }

    [HttpPost]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<IActionResult> Create([FromBody] CreateCourse request)
    {
        var courseDto = await _courseService.CreateCurseAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = courseDto.Id }, courseDto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourse request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var authorId = Guid.Parse(userIdString);

        var result = await _courseService.UpdateCourseAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _courseService.DeleteAsync(id);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/restore")]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _courseService.RestoreAsync(id);
        return Ok(result);
    }

    [HttpPost("courses/{courseId}/join")]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<IActionResult> EnrollStudent(Guid courseId, [FromBody] Guid studentId)
    {
        try
        {
            await _courseService.EnrollUserAsync(courseId, studentId);
            return Ok("Студента успішно додано до курсу.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Сталася внутрішня помилка сервера.");
        }
    }
}
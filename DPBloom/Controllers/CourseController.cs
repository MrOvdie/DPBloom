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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetAll()
    {
        var courses = await _courseService.GetAllAsync();
        return Ok(courses);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<CourseDto>> GetById(Guid id)
    {
        var course = await _courseService.GetByIdWithAccessAsync(id); //TODO: check
        if (course is null) return NotFound();

        return Ok(course);
    }

    [HttpGet("content/{courseId:guid}")]
    [Authorize]
    public async Task<ActionResult<CourseAggregateDto>> GetCourseContent(Guid courseId)
    {
        var course = await _courseService.GetCourseContentAsync(courseId);
        if (course is null) return NotFound();
        
        return Ok(course);
    }
    
    [HttpGet("name/{courseName}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetCourseByName(string courseName)
    {
        var courses = await _courseService.GetCourseByNameAsync(courseName);
        if (courses is null) return NotFound(); //TODO: maybe add checking of enrollment and so
        
        return Ok(courses);
    }
    
    [HttpGet("author/{courseAuthorId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetCoursesByAuthor(Guid courseAuthorId)
    {
        var courses = await _courseService.GetCourseByAuthorAsync(courseAuthorId);
        if (courses is null) return NotFound();
        
        return Ok(courses);
    }

    [HttpGet("enrolled")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetEnrolledCourses()
    {
        var courses = await _courseService.GetEnrolledCoursesAsync();
        if (courses is null) return NotFound();
        
        return Ok(courses);
    }

    [HttpPost]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<ActionResult<CourseDto>> Create([FromBody] CreateCourseDto request)
    {
        var courseDto = await _courseService.CreateCurseAsync(request);
        
        return CreatedAtAction(nameof(GetById), new { id = courseDto.Id }, courseDto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<ActionResult<CourseDto>> Update(Guid id, [FromBody] UpdateCourseDto request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var authorId = Guid.Parse(userIdString);

        var result = await _courseService.UpdateCourseAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<ActionResult<CourseDto>> Delete(Guid id)
    {
        var result = await _courseService.DeleteAsync(id);
        
        return Ok(result);
    }

    [HttpPatch("{id:guid}/restore")]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<ActionResult<CourseDto>> Restore(Guid id)
    {
        var result = await _courseService.RestoreAsync(id);
       
        return Ok(result);
    }

    [HttpPost("{courseId}/enroll/{userId:guid}")]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<IActionResult> EnrollStudent(Guid courseId, Guid userId)
    {
        try
        {
            await _courseService.EnrollUserAsync(courseId, userId);
            return Ok("User successfully enrolled in the course.");
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
            return StatusCode(500, "Internal server error.");
        }
    }
    
    [HttpPost("dismiss/{enrollmentId:guid}")]
    [Authorize(Policy = "RequireTeacherPrivileges")]
    public async Task<IActionResult> DismissStudent(Guid enrollmentId)
    {
        try
        {
            await _courseService.DismissUserAsync(enrollmentId);
            return Ok("User successfully dismissed from the course.");
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
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("statistics/{courseId:guid}/exams-progress/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<double>> GetExamsProgressionPercent(Guid courseId, Guid userId)
    {
        var result = await _courseService.GetCourseExamsProgressPercentAsync(courseId, userId);
        
        return Ok(result);
    }
    
    [HttpGet("statistics/{courseId:guid}/course-progress/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<double>> GetCourseProgressionPercent(Guid courseId, Guid userId)
    {
        var result = await _courseService.GetCourseScoreProgressPercentAsync(courseId, userId);
        
        return Ok(result);
    }
    
    [HttpGet("statistics/{courseId:guid}/course-score/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<double>> GetCourseScore(Guid courseId, Guid userId)
    {
        var result = await _courseService.GetUserCourseScoreAsync(courseId, userId);
        
        return Ok(result);
    }

    [HttpPost("statistics/aggregated/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<AggregatedCourseStatsDto>>> GetStatisticsForAllEnrolledCourses(Guid userId, [FromBody] List<Guid> courseIds)
    {
        var result = await _courseService.GetStatisticsForCoursesAsync(userId, courseIds);
        
        return Ok(result);
    }
}
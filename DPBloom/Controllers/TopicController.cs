using DPBloom.Application.Topic;
using DPBloom.Application.Topic.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPBloom.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TopicController : ControllerBase
{
    private readonly ITopicService _topicService;

    public TopicController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<TopicDto>>> GetAll()
    {
        var topics = await _topicService.GetAllAsync();
        
        return Ok(topics);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<TopicDto>> GetById(Guid id)
    {
        var topic = await _topicService.GetByIdWithAccessAsync(id);
        if (topic is null) return NotFound();

        return Ok(topic);
    }
    
    [HttpGet("name/{topicName}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<TopicDto>>> GetLectureByName(string topicName)
    {
        var lecture = await _topicService.GetTopicByNameAsync(topicName);
        
        return Ok(lecture);
    }
    
    [HttpGet("course/{courseId:guid}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<TopicDto>>> GetLectureByCourse(Guid courseId)
    {
        var lecture = await _topicService.GetTopicsByCourseAsync(courseId);
        
        return Ok(lecture);
    }
    
    [HttpGet("author/{authorId:guid}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<TopicDto>>> GetLectureByAuthor(Guid authorId)
    {
        var lecture = await _topicService.GetTopicsByAuthorAsync(authorId);
        
        return Ok(lecture);
    }

    [HttpPost("{courseId:guid}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<ActionResult<TopicDto>> Create(Guid courseId, [FromBody] CreateTopicDto request)
    {
        var topicDto = await _topicService.CreateAsync(courseId, request);
        
        return CreatedAtAction(nameof(GetById), new { id = topicDto.Id }, topicDto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<ActionResult<TopicDto>> Update(Guid id, [FromBody] UpdateTopicDto request)
    {
        var result = await _topicService.UpdateAsync(id, request);
       
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<ActionResult<TopicDto>> Delete(Guid id)
    {
        var result = await _topicService.DeleteAsync(id);
        
        return Ok(result);
    }
    
    [HttpPatch("{id:guid}/restore")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<ActionResult<TopicDto>> Restore(Guid id)
    {
        var result = await _topicService.RestoreAsync(id);
        
        return Ok(result);
    }
}
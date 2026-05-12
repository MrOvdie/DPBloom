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
    public async Task<IActionResult> GetAll()
    {
        var topics = await _topicService.GetAllAsync();
        return Ok(topics);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var topic = await _topicService.GetByIdAsync(id);
        if (topic is null) return NotFound();

        return Ok(topic);
    }

    [HttpPost("{courseId:guid}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<IActionResult> Create(Guid courseId, [FromBody] CreateTopic request)
    {
        var topicDto = await _topicService.CreateAsync(courseId, request);
        return CreatedAtAction(nameof(GetById), new { id = topicDto.Id }, topicDto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTopic request)
    {
        var result = await _topicService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _topicService.DeleteAsync(id);
        return Ok(result);
    }
    
    [HttpPatch("{id:guid}/restore")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _topicService.RestoreAsync(id);
        return Ok(result);
    }
}
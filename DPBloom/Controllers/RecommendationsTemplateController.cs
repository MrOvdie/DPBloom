using DPBloom.Application.Bloom;
using DPBloom.Application.Bloom.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPBloom.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecommendationsTemplateController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsTemplateController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<RecommendationTemplateDto>>> GetAll()
    {
        var result = await _recommendationService.GetRecommendationsAsync();

        return Ok(result);
    }
    
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RecommendationTemplateDto>> GetAll(Guid id)
    {
        var result = await _recommendationService.GetRecommendationByIdAsync(id);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RecommendationTemplateDto>> Create([FromBody] CreateRecommendationTemplate request)
    {
        var result = await _recommendationService.CreateAsync(request);
        
        return Ok(result);
    }
    
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RecommendationTemplateDto>> Create(Guid id, [FromBody] UpdateRecommendationTemplate request)
    {
        var result = await _recommendationService.UpdateAsync(id, request);
        
        return Ok(result);
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RecommendationTemplateDto>> Delete(Guid id)
    {
        var result = await _recommendationService.DeleteAsync(id);
        
        return Ok(result);
    }
    
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RecommendationTemplateDto>> Restore(Guid id)
    {
        var result = await _recommendationService.RestoreAsync(id);
        
        return Ok(result);
    }
}
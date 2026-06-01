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
    private readonly IRecommendationTemplateService _recommendationTemplateService;

    public RecommendationsTemplateController(IRecommendationTemplateService recommendationTemplateService)
    {
        _recommendationTemplateService = recommendationTemplateService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<RecommendationTemplateDto>>> GetAll()
    {
        var result = await _recommendationTemplateService.GetRecommendationTemplatesAsync();

        return Ok(result);
    }
    
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RecommendationTemplateDto>> GetAll(Guid id)
    {
        var result = await _recommendationTemplateService.GetRecommendationTemplateByIdAsync(id);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RecommendationTemplateDto>> Create([FromBody] CreateRecommendationTemplateDto request)
    {
        var result = await _recommendationTemplateService.CreateAsync(request);
        
        return Ok(result);
    }
    
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RecommendationTemplateDto>> Create(Guid id, [FromBody] UpdateRecommendationTemplateDto request)
    {
        var result = await _recommendationTemplateService.UpdateAsync(id, request);
        
        return Ok(result);
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RecommendationTemplateDto>> Delete(Guid id)
    {
        var result = await _recommendationTemplateService.DeleteAsync(id);
        
        return Ok(result);
    }
    
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RecommendationTemplateDto>> Restore(Guid id)
    {
        var result = await _recommendationTemplateService.RestoreAsync(id);
        
        return Ok(result);
    }
}
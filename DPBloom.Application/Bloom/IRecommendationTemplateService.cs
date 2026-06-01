using DPBloom.Application.Bloom.Contracts;

namespace DPBloom.Application.Bloom;

public interface IRecommendationTemplateService
{
    Task<IReadOnlyList<RecommendationTemplateDto>> GetRecommendationTemplatesAsync();
    Task<RecommendationTemplateDto> GetRecommendationTemplateByIdAsync(Guid id);
    Task<RecommendationTemplateDto> CreateAsync(CreateRecommendationTemplateDto createTemplateDto);
    Task<RecommendationTemplateDto> UpdateAsync(Guid id, UpdateRecommendationTemplateDto updateTemplateDto);
    Task<RecommendationTemplateDto> DeleteAsync(Guid id);
    Task<RecommendationTemplateDto> RestoreAsync(Guid id);
}
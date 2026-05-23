using DPBloom.Application.Bloom.Contracts;

namespace DPBloom.Application.Bloom;

public interface IRecommendationService
{
    Task<IReadOnlyList<RecommendationTemplateDto>> GetRecommendationsAsync();
    Task<RecommendationTemplateDto> GetRecommendationByIdAsync(Guid id);
    Task<RecommendationTemplateDto> CreateAsync(CreateRecommendationTemplate createTemplate);
    Task<RecommendationTemplateDto> UpdateAsync(Guid id, UpdateRecommendationTemplate updateTemplate);
    Task<RecommendationTemplateDto> DeleteAsync(Guid id);
    Task<RecommendationTemplateDto> RestoreAsync(Guid id);
}
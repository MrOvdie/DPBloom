using DPBloom.Application.Bloom.Contracts;

namespace DPBloom.Application.Bloom;

public interface IRecommendationTemplateService
{
    Task<IReadOnlyList<RecommendationTemplateDto>> GetRecommendationTemplatesAsync();
    Task<RecommendationTemplateDto> GetRecommendationTemplateByIdAsync(Guid id);
    Task<RecommendationTemplateDto> CreateAsync(CreateRecommendationTemplate createTemplate);
    Task<RecommendationTemplateDto> UpdateAsync(Guid id, UpdateRecommendationTemplate updateTemplate);
    Task<RecommendationTemplateDto> DeleteAsync(Guid id);
    Task<RecommendationTemplateDto> RestoreAsync(Guid id);
}
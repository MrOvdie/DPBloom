using AutoMapper;
using DPBloom.Application.Bloom.Contracts;
using DPBloom.Core.Bloom;

namespace DPBloom.Application.Bloom;

public class RecommendationService : IRecommendationService
{
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IMapper _mapper;

    public RecommendationService(IRecommendationRepository recommendationRepository, IMapper mapper)
    {
        _recommendationRepository = recommendationRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RecommendationTemplateDto>> GetRecommendationsAsync()
    {
        var recommendations = await _recommendationRepository.GetAllAsync();
        if (recommendations is null)
            throw new KeyNotFoundException("Recommendations not found");
        
        return _mapper.Map<List<RecommendationTemplateDto>>(recommendations);
    }
    
    public async Task<RecommendationTemplateDto> GetRecommendationByIdAsync(Guid id)
    {
        var recommendation = await _recommendationRepository.GetByIdAsync(id);
        if (recommendation is null)
            throw new KeyNotFoundException("Recommendations not found");
        
        return _mapper.Map<RecommendationTemplateDto>(recommendation);
    }
    
    public async Task<RecommendationTemplateDto> CreateAsync(CreateRecommendationTemplate createTemplate)
    {
       var createTemplateModel = _mapper.Map<RecommendationTemplateModel>(createTemplate);
    
        await _recommendationRepository.AddAsync(createTemplateModel);
    
        return _mapper.Map<RecommendationTemplateDto>(createTemplateModel);
    }

    public async Task<RecommendationTemplateDto> UpdateAsync(Guid id, UpdateRecommendationTemplate updateTemplate)
    {
        var existingRecommendation = await _recommendationRepository.GetByIdAsync(id);
        if (existingRecommendation is null)
            throw new KeyNotFoundException("Recommendation not found");

        var updatedRecommendationTemplateModel = _mapper.Map(updateTemplate, existingRecommendation);
        updatedRecommendationTemplateModel.UpdatedOn = DateTime.UtcNow;
        
        await _recommendationRepository.UpdateAsync(existingRecommendation);
    
        return _mapper.Map<RecommendationTemplateDto>(existingRecommendation);
    }

    public async Task<RecommendationTemplateDto> DeleteAsync(Guid id)
    {
        var recommendation = await _recommendationRepository.GetByIdAsync(id);
        if (recommendation is null)
            throw new KeyNotFoundException("Recommendation not found");

        await _recommendationRepository.DeleteAsync(id);
        
        return _mapper.Map<RecommendationTemplateDto>(recommendation);
    }

    public async Task<RecommendationTemplateDto> RestoreAsync(Guid id)
    {
        var recommendation = await _recommendationRepository.RestoreAsync(id);
        if (recommendation is null)
            throw new KeyNotFoundException("Deleted recommendation not found");

        return _mapper.Map<RecommendationTemplateDto>(recommendation);
    }
}
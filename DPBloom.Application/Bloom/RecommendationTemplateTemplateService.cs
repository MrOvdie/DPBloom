using AutoMapper;
using DPBloom.Application.Bloom.Contracts;
using DPBloom.Core.Bloom;
using UUIDNext;

namespace DPBloom.Application.Bloom;

public class RecommendationTemplateTemplateService : IRecommendationTemplateService
{
    private readonly IRecommendationTemplateRepository _recommendationTemplateRepository;
    private readonly IMapper _mapper;

    public RecommendationTemplateTemplateService(IRecommendationTemplateRepository recommendationTemplateRepository, IMapper mapper)
    {
        _recommendationTemplateRepository = recommendationTemplateRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RecommendationTemplateDto>> GetRecommendationTemplatesAsync()
    {
        var recommendations = await _recommendationTemplateRepository.GetAllAsync();
        if (recommendations is null)
            throw new KeyNotFoundException("Recommendations not found");

        return _mapper.Map<List<RecommendationTemplateDto>>(recommendations);
    }

    public async Task<RecommendationTemplateDto> GetRecommendationTemplateByIdAsync(Guid id)
    {
        var recommendation = await _recommendationTemplateRepository.GetByIdAsync(id);
        if (recommendation is null)
            throw new KeyNotFoundException("Recommendations not found");

        return _mapper.Map<RecommendationTemplateDto>(recommendation);
    }

    public async Task<RecommendationTemplateDto> CreateAsync(CreateRecommendationTemplate createTemplate)
    {
        var createTemplateModel = _mapper.Map<RecommendationTemplateModel>(createTemplate);
        createTemplateModel.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);
        createTemplateModel.CreatedOn = createTemplateModel.UpdatedOn = DateTime.UtcNow;

        await _recommendationTemplateRepository.AddAsync(createTemplateModel);

        return _mapper.Map<RecommendationTemplateDto>(createTemplateModel);
    }

    public async Task<RecommendationTemplateDto> UpdateAsync(Guid id, UpdateRecommendationTemplate updateTemplate)
    {
        var existingRecommendation = await _recommendationTemplateRepository.GetByIdAsync(id);
        if (existingRecommendation is null)
            throw new KeyNotFoundException("Recommendation not found");

        var updatedRecommendationTemplateModel = _mapper.Map(updateTemplate, existingRecommendation);
        updatedRecommendationTemplateModel.UpdatedOn = DateTime.UtcNow;

        await _recommendationTemplateRepository.UpdateAsync(existingRecommendation);

        return _mapper.Map<RecommendationTemplateDto>(existingRecommendation);
    }

    public async Task<RecommendationTemplateDto> DeleteAsync(Guid id)
    {
        var recommendation = await _recommendationTemplateRepository.GetByIdAsync(id);
        if (recommendation is null)
            throw new KeyNotFoundException("Recommendation not found");

        await _recommendationTemplateRepository.DeleteAsync(id);

        return _mapper.Map<RecommendationTemplateDto>(recommendation);
    }

    public async Task<RecommendationTemplateDto> RestoreAsync(Guid id)
    {
        var recommendation = await _recommendationTemplateRepository.RestoreAsync(id);
        if (recommendation is null)
            throw new KeyNotFoundException("Deleted recommendation not found");

        return _mapper.Map<RecommendationTemplateDto>(recommendation);
    }
}
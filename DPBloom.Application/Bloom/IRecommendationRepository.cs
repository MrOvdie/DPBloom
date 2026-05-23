using DPBloom.Application.Base;
using DPBloom.Core.Bloom;
using DPBloom.Core.Exam.Enums;

namespace DPBloom.Application.Bloom;

public interface IRecommendationRepository : IRepository<RecommendationTemplateModel>
{
    Task<IReadOnlyList<RecommendationTemplateModel>> GetTemplatesByLevelsAsync(List<BloomLevel> levels, Guid? courseId = null);
}
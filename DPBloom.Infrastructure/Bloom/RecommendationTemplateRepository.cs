using AutoMapper;
using DPBloom.Application.Bloom;
using DPBloom.Core.Bloom;
using DPBloom.Core.Exam.Enums;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Bloom;

public class RecommendationTemplateRepository : RepositoryBase<RecommendationTemplateModel, RecommendationTemplateDao, ApplicationDbContext>, IRecommendationTemplateRepository
{
    public RecommendationTemplateRepository(ApplicationDbContext context, IMapper mapper) 
        : base(context, mapper)
    {
    }

    public async Task<IReadOnlyList<RecommendationTemplateModel>> GetTemplatesByLevelsAsync(List<BloomLevel> levels, Guid? courseId = null)
    {
        if (levels is null || !levels.Any())
            return new List<RecommendationTemplateModel>();

        var query = DbContext.Set<RecommendationTemplateDao>()
            .Where(t => levels.Contains(t.TargetLevel));

        if (courseId.HasValue)
        {
            query = query.Where(t => t.CourseId == courseId.Value || t.CourseId == null);
        }
        else
        {
            query = query.Where(t => t.CourseId == null);
        }

        var templates = await query.ToListAsync();

        var recommendations = templates
            .GroupBy(t => t.TargetLevel)
            .Select(group => group.OrderByDescending(t => t.CourseId.HasValue).First())
            .ToList();
        
        return Mapper.Map<List<RecommendationTemplateModel>>(recommendations);
    }
}
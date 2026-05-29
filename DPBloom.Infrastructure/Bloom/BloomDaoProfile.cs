using AutoMapper;
using DPBloom.Core.Bloom;

namespace DPBloom.Infrastructure.Bloom;

public class BloomDaoProfile : Profile
{
    public BloomDaoProfile()
    {
        CreateMap<RecommendationTemplateDao, RecommendationTemplateModel>().ReverseMap();
        
        CreateMap<RecommendedMaterial, RecommendedMaterialDao>().ReverseMap();
        
        CreateMap<BloomAnalysisModel, BloomAnalysisDao>().ReverseMap();
        
        CreateMap<BloomLevelPerformance, BloomLevelPerformanceDao>().ReverseMap();
    }
}
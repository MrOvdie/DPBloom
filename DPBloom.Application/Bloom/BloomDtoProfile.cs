using AutoMapper;
using DPBloom.Application.Bloom.Contracts;
using DPBloom.Core.Bloom;
using UUIDNext;

namespace DPBloom.Application.Bloom;

public class BloomDtoProfile : Profile
{
    public BloomDtoProfile()
    {
        CreateMap<BloomAnalysisModel, BloomAnalysisDto>();
        CreateMap<BloomLevelPerformance, BloomLevelPerformanceDto>();
        CreateMap<RecommendedMaterial, RecommendedMaterialDto>();
        
        CreateMap<RecommendedMaterial, RecommendationDto>();
        CreateMap<RecommendationTemplateModel, RecommendationTemplateDto>();

        CreateMap<CreateRecommendationTemplateDto, RecommendationTemplateModel>();
        
        CreateMap<UpdateRecommendationTemplateDto, RecommendationTemplateModel>()
            .ForAllMembers(opt =>
                opt.Condition((_, _, srcMember) =>
                    srcMember is not null));
    }
}
using AutoMapper;
using DPBloom.Core.Bloom;

namespace DPBloom.Infrastructure.Bloom;

public class BloomDaoProfile : Profile
{
    public BloomDaoProfile()
    {
        CreateMap<RecommendationTemplateDao, RecommendationTemplateModel>().ReverseMap();
    }
}
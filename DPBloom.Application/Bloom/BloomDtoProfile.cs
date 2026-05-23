using AutoMapper;
using DPBloom.Application.Bloom.Contracts;
using DPBloom.Core.Bloom;
using UUIDNext;

namespace DPBloom.Application.Bloom;

public class BloomDtoProfile : Profile
{
    public BloomDtoProfile()
    {
        CreateMap<RecommendedMaterial, RecommendationDto>();
        CreateMap<RecommendationTemplateModel, RecommendationTemplateDto>();

        CreateMap<CreateRecommendationTemplate, RecommendationTemplateModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));
        
        CreateMap<UpdateRecommendationTemplate, RecommendationTemplateModel>()
            .ForAllMembers(opt =>
                opt.Condition((_, _, srcMember) =>
                    srcMember is not null));
    }
}
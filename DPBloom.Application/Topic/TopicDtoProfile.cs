using AutoMapper;
using DPBloom.Application.Topic.Contracts;
using DPBloom.Core.Topic;
using UUIDNext;

namespace DPBloom.Application.Topic;

public class TopicDtoProfile : Profile
{
    public TopicDtoProfile()
    {
        CreateMap<TopicDto, TopicModel>().ReverseMap();
        CreateMap<CreateTopic, TopicModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));
        CreateMap<UpdateTopic, TopicModel>()
            .ForAllMembers(opt =>
                opt.Condition((_, _, srcMember) =>
                    srcMember is not null));
    }
}
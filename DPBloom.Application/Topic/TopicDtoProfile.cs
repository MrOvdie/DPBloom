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
        CreateMap<CreateTopicDto, TopicModel>();
        CreateMap<UpdateTopicDto, TopicModel>()
            .ForAllMembers(opt =>
                opt.Condition((_, _, srcMember) =>
                    srcMember is not null));
    }
}
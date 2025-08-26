using AutoMapper;
using DPBloom.Application.Topic.Contracts;
using DPBloom.Core.Topic;

namespace DPBloom.Application.Topic;

public class TopicDtoProfile : Profile
{
    public TopicDtoProfile()
    {
        CreateMap<TopicDto, TopicModel>().ReverseMap();
        CreateMap<CreateTopic, TopicModel>();
        CreateMap<UpdateTopic, TopicModel>()
            .ForAllMembers(opt =>
                opt.Condition((_, _, srcMember) =>
                    srcMember != null));
    }
}
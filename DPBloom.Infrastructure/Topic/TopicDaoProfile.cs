using AutoMapper;
using DPBloom.Core.Topic;

namespace DPBloom.Infrastructure.Topic;

public class TopicDaoProfile : Profile
{
    public TopicDaoProfile()
    {
        CreateMap<TopicDao, TopicModel>().ReverseMap();
    }
}
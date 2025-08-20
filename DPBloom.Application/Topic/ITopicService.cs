using DPBloom.Application.Base;
using DPBloom.Application.Topic.Contracts;

namespace DPBloom.Application.Topic;

public interface ITopicService : ICrud<TopicDto>
{
    Task<IEnumerable<TopicDto>> GetTopicByNameAsync(string topicName);
    Task<IEnumerable<TopicDto>> GetTopicByAuthorAsync(string authorId);
    Task<TopicDto> CreateAsync(CreateTopic createTopic);
    Task UpdateAsync(UpdateTopic updateTopic);
}
using DPBloom.Application.Base;
using DPBloom.Application.Topic.Contracts;
using DPBloom.Core.Topic;

namespace DPBloom.Application.Topic;

public interface ITopicService : ICrud<TopicDto>
{
    Task<IEnumerable<TopicDto>> GetTopicByNameAsync(string topicName);
    Task<IEnumerable<TopicDto>> GetTopicByAuthorAsync(Guid authorId);
    Task<TopicDto> CreateAsync(Guid courseId, CreateTopic createTopic);
    Task<TopicDto> UpdateAsync(Guid topicId, UpdateTopic updateTopic);
    Task<TopicModel> GetEntityByIdAsync(Guid id);
}
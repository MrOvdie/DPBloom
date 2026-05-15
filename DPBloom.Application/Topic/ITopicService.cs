using DPBloom.Application.Base;
using DPBloom.Application.Topic.Contracts;
using DPBloom.Core.Topic;

namespace DPBloom.Application.Topic;

public interface ITopicService : ICrud<TopicDto>
{
    Task<TopicDto> GetByIdWithAccessAsync(Guid topicId);
    Task<IReadOnlyList<TopicDto>> GetTopicByNameAsync(string topicName);
    Task<IReadOnlyList<TopicDto>> GetTopicsByAuthorAsync(Guid authorId);
    Task<IReadOnlyList<TopicDto>> GetTopicsByCourseAsync(Guid courseId);
    Task<TopicDto> CreateAsync(Guid courseId, CreateTopic createTopic);
    Task<TopicDto> UpdateAsync(Guid topicId, UpdateTopic updateTopic);
}
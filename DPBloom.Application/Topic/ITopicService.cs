using DPBloom.Application.Topic.Contracts;

namespace DPBloom.Application.Topic;

public interface ITopicService
{
    //TODO: write ICrud<TModel>
    Task<IEnumerable<TopicDto>> GetTopicsListAsync();
    Task<TopicDto> GetTopicByIdAsync(string topicId);
    Task<IEnumerable<TopicDto>> GetTopicByNameAsync(string topicName);
    Task<IEnumerable<TopicDto>> GetTopicByAuthorAsync(string authorId);
    Task<TopicDto> CreateAsync(CreateTopic createTopic);
    Task UpdateAsync(UpdateTopic updateTopic);
    Task DeleteAsync(string topicId);
    Task RestoreAsync(string lectureId);
}
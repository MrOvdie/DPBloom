using DPBloom.Application.Base;
using DPBloom.Core.Topic;

namespace DPBloom.Application.Topic;

public interface ITopicRepository : IRepository<TopicModel>
{
    Task<IReadOnlyList<TopicModel>> GetByCourseAsync(Guid courseId);
    Task<IReadOnlyList<TopicModel>> GetByAuthorAsync(Guid courseId);
    Task<Guid?> GetCourseIdByTopicIdAsync(Guid topicId);
    Task<bool> IsTopicAuthorAsync(Guid topicId, Guid userId);
}
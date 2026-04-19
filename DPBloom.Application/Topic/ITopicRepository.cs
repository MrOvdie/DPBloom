using DPBloom.Application.Base;
using DPBloom.Core.Topic;

namespace DPBloom.Application.Topic;

public interface ITopicRepository : IRepository<TopicModel>
{
    Task<IReadOnlyList<TopicModel>> GetByCourseAsync(string courseId);
    Task<IReadOnlyList<TopicModel>> GetByAuthorAsync(string courseId);
    
}
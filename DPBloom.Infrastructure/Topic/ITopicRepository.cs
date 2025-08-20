using DPBloom.Core.Topic;
using DPBloom.Infrastructure.Base;

namespace DPBloom.Infrastructure.Topic;

public interface ITopicRepository : IRepository<TopicModel, TopicDao>
{
    Task<IReadOnlyList<TopicModel>> GetByCourseAsync(string courseId);
    
}
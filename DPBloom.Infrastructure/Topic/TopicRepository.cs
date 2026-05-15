using AutoMapper;
using DPBloom.Application.Topic;
using DPBloom.Core.Topic;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Topic;

public class TopicRepository : RepositoryBase<TopicModel,TopicDao, ApplicationDbContext>,  ITopicRepository
{ 
    public TopicRepository(ApplicationDbContext context, IMapper mapper) 
        : base(context, mapper)
    {
    }

    public async Task<IReadOnlyList<TopicModel>> GetByCourseAsync(Guid courseId)
    {
        var topicsByCourse = await DbContext.Set<TopicDao>()
            .Where(l => l.CourseId.Equals(courseId))
            .ToListAsync();
        
        return Mapper.Map<List<TopicModel>>(topicsByCourse);
    }

    public async Task<IReadOnlyList<TopicModel>> GetByAuthorAsync(Guid authorId)
    {
        var topicsByAuthor = await DbContext.Set<TopicDao>()
            .Where(l => l.AuthorId.Equals(authorId))
            .ToListAsync();
        
        return Mapper.Map<List<TopicModel>>(topicsByAuthor);
    }
    
    public async Task<Guid?> GetCourseIdByTopicIdAsync(Guid topicId)
    {
        return await DbContext.Topics
            .Where(l => l.Id == topicId)
            .Select(l => l.CourseId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsTopicAuthorAsync(Guid topicId, Guid userId)
    {
        return await DbContext.Topics
            .AnyAsync(c => c.Id == topicId && c.AuthorId == userId);
    }
}
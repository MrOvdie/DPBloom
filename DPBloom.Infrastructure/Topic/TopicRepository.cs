using AutoMapper;
using DPBloom.Application.Topic;
using DPBloom.Core.Course;
using DPBloom.Core.Topic;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Course;
using DPBloom.Infrastructure.Data;
using DPBloom.Infrastructure.Exam;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Topic;

public class TopicRepository : RepositoryBase<TopicModel,TopicDao, ApplicationDbContext>,  ITopicRepository
{ 
    public TopicRepository(ApplicationDbContext context, IMapper mapper) 
        : base(context, mapper)
    {
    }

    public async Task<IReadOnlyList<TopicModel>> GetByCourseAsync(string courseId)
    {
        var topicsByCourse = await DbContext.Set<TopicDao>()
            .Where(l => l.CourseId.Equals(courseId))
            .ToListAsync();
        
        return Mapper.Map<IReadOnlyList<TopicModel>>(topicsByCourse);
    }

    public async Task<IReadOnlyList<TopicModel>> GetByAuthorAsync(string authorId)
    {
        var topicsByAuthor = await DbContext.Set<TopicDao>()
            .Where(l => l.AuthorId.Equals(authorId))
            .ToListAsync();
        
        return Mapper.Map<IReadOnlyList<TopicModel>>(topicsByAuthor);
    }
}
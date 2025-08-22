using AutoMapper;
using DPBloom.Core.Lecture;
using DPBloom.Infrastructure.Base;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Lecture;

public class LectureRepository : RepositoryBase<LectureModel, LectureDao, ApplicationDbContext>, ILectureRepository
{
    public LectureRepository(ApplicationDbContext context, IMapper mapper) 
        : base(context, mapper)
    {
    }

    public async Task<IReadOnlyList<LectureModel>> GetByCourseAsync(string courseId)
    {
        var lecturesByCourse = await DbContext.Set<LectureDao>()
            .Where(l => l.CourseId.Equals(courseId))
            .ToListAsync();
        return Mapper.Map<IReadOnlyList<LectureModel>>(lecturesByCourse);
    }

    public async Task<IReadOnlyList<LectureModel>> GetByTopicAsync(string topicId)
    {
        var lecturesByTopic = await DbContext.Set<LectureDao>()
            .Where(l => l.TopicId.Equals(topicId))
            .ToListAsync();
        return Mapper.Map<IReadOnlyList<LectureModel>>(lecturesByTopic);
    }
}
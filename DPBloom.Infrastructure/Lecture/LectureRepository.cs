using AutoMapper;
using DPBloom.Application.Lecture;
using DPBloom.Core.Lecture;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Lecture;

public class LectureRepository : RepositoryBase<LectureModel, LectureDao, ApplicationDbContext>, ILectureRepository
{
    public LectureRepository(ApplicationDbContext context, IMapper mapper)
        : base(context, mapper)
    {
    }

    public async Task<IReadOnlyList<LectureModel>> GetByCourseAsync(Guid courseId)
    {
        var lecturesByCourse = await DbContext.Lectures
            .Where(l => l.CourseId.Equals(courseId))
            .ToListAsync();
        return Mapper.Map<List<LectureModel>>(lecturesByCourse);
    }

    public async Task<IReadOnlyList<LectureModel>> GetByTopicAsync(Guid topicId)
    {
        var lecturesByTopic = await DbContext.Lectures
            .Where(l => l.TopicId.Equals(topicId))
            .ToListAsync();
        return Mapper.Map<List<LectureModel>>(lecturesByTopic);
    }
}
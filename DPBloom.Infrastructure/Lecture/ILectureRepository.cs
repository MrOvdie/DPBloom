using DPBloom.Core.Lecture;
using DPBloom.Infrastructure.Base;

namespace DPBloom.Infrastructure.Lecture;

public interface ILectureRepository : IRepository<LectureModel, LectureDao>
{
    Task<IReadOnlyList<LectureModel>> GetByCourseAsync(string courseId);
    Task<IReadOnlyList<LectureModel>> GetByTopicAsync(string topicId);
}
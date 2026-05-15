using DPBloom.Application.Base;
using DPBloom.Core.Lecture;

namespace DPBloom.Application.Lecture;

public interface ILectureRepository : IRepository<LectureModel>
{
    Task<IReadOnlyList<LectureModel>> GetByCourseAsync(Guid courseId);
    Task<IReadOnlyList<LectureModel>> GetByTopicAsync(Guid topicId);
    Task<Guid?> GetCourseIdByLectureIdAsync(Guid lectureId);
    Task<bool> IsLectureAuthorAsync(Guid lectureId, Guid userId);
}
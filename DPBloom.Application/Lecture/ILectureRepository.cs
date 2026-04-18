using DPBloom.Application.Base;
using DPBloom.Core.Lecture;
using DPBloom.Infrastructure.Lecture;

namespace DPBloom.Application.Lecture;

public interface ILectureRepository : IRepository<LectureModel>
{
    Task<IReadOnlyList<LectureModel>> GetByCourseAsync(Guid courseId);
    Task<IReadOnlyList<LectureModel>> GetByTopicAsync(Guid topicId);
}
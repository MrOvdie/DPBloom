using DPBloom.Application.Base;
using DPBloom.Application.Lecture.Contracts;
using DPBloom.Core.Lecture;

namespace DPBloom.Application.Lecture;

public interface ILectureService : ICrud<LectureDto>
{
    Task<IEnumerable<LectureDto>> GetLectureByNameAsync(string lectureName);
    Task<IEnumerable<LectureDto>> GetLecturesByCourseAsync(Guid courseId);
    Task<IEnumerable<LectureDto>> GetLecturesByTopicAsync(Guid topicId);
    Task<IEnumerable<LectureDto>> GetLectureByAuthorAsync(Guid authorId);
    Task<LectureDto> CreateAsync(CreateLecture createLecture);
    Task<LectureDto> UpdateAsync(Guid lectureId, UpdateLecture updateLecture);
    Task<LectureModel> GetEntityByIdAsync(Guid id);
}
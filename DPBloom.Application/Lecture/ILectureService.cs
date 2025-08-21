using DPBloom.Application.Base;
using DPBloom.Application.Lecture.Contracts;
using DPBloom.Core.Lecture;

namespace DPBloom.Application.Lecture;

public interface ILectureService : ICrud<LectureDto>
{
    Task<IEnumerable<LectureDto>> GetLectureByNameAsync(string lectureName);
    Task<IEnumerable<LectureDto>> GetLecturesByCourseAsync(string courseId);
    Task<IEnumerable<LectureDto>> GetLecturesByTopicAsync(string topicId);
    Task<IEnumerable<LectureDto>> GetLectureByAuthorAsync(string authorId);
    Task<LectureDto> CreateAsync(CreateLecture createLecture);
    Task<LectureDto> UpdateAsync(string lectureId, UpdateLecture updateLecture);

}
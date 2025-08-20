using DPBloom.Application.Lecture.Contracts;
using DPBloom.Core.Lecture;

namespace DPBloom.Application.Lecture;

public interface ILectureService 
{
    Task<IEnumerable<LectureDto>> GetLecturesListAsync();
    Task<LectureDto> GetLectureByIdAsync(string lectureId);
    Task<IEnumerable<LectureDto>> GetLectureByNameAsync(string lectureName);
    Task<IEnumerable<LectureDto>> GetLecturesByCourseAsync(string courseId);
    Task<IEnumerable<LectureDto>> GetLecturesByTopicAsync(string topicId);
    Task<IEnumerable<LectureDto>> GetLectureByAuthorAsync(string authorId);
    Task<LectureDto> Create(CreateLecture createLecture);
    Task UpdateAsync(UpdateLecture updateLecture);
    Task DeleteAsync(string lectureId);
    Task RestoreAsync(string lectureId);
}
using DPBloom.Application.Base;
using DPBloom.Application.Lecture.Contracts;
using DPBloom.Core.Lecture;

namespace DPBloom.Application.Lecture;

public interface ILectureService : ICrud<LectureDto>
{
    Task<LectureDetailsDto> GetDetailsByIdWithAccessAsync(Guid lectureId);
    Task<IReadOnlyList<LectureDto>> GetLectureByNameAsync(string lectureName);
    Task<IReadOnlyList<LectureDto>> GetLecturesByCourseAsync(Guid courseId);
    Task<IReadOnlyList<LectureDto>> GetLecturesByTopicAsync(Guid topicId);
    Task<IReadOnlyList<LectureDto>> GetLectureByAuthorAsync(Guid authorId);
    Task<LectureDetailsDto> CreateAsync(Guid courseId, CreateLectureDto createLectureDto);
    Task<LectureDetailsDto> UpdateAsync(Guid lectureId, UpdateLectureDto updateLectureDto);
}
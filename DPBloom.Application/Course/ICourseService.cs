using DPBloom.Application.Base;
using DPBloom.Application.Course.Contracts;
using DPBloom.Core.Course;

namespace DPBloom.Application.Course;

public interface ICourseService : ICrud<CourseDto>
{
    Task<IEnumerable<CourseDto>> GetCourseByNameAsync(string courseName);
    Task<IEnumerable<CourseDto>> GetCourseByAuthorAsync(Guid authorId);
    Task<CourseDto> CreateCurseAsync(CreateCourse createCourse);
    Task<CourseDto> UpdateCourseAsync(Guid courseId, UpdateCourse updateCourse);
    Task<CourseAggregateDto?> GetCourseContentAsync(Guid courseId, bool bypassAccessCheck = false);
    Task EnrollUserAsync(Guid courseId, Guid userId);
    Task DismissUserAsync(Guid enrollmentId);
}
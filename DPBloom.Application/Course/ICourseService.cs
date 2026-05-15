using DPBloom.Application.Base;
using DPBloom.Application.Course.Contracts;
using DPBloom.Core.Course;

namespace DPBloom.Application.Course;

public interface ICourseService : ICrud<CourseDto>
{
    Task<CourseDto> GetByIdWithAccessAsync(Guid courseId);
    Task<IReadOnlyList<CourseDto>> GetCourseByNameAsync(string courseName);
    Task<IReadOnlyList<CourseDto>> GetCourseByAuthorAsync(Guid authorId);
    Task<IReadOnlyList<CourseDto>> GetEnrolledCoursesAsync();
    Task<CourseDto> CreateCurseAsync(CreateCourse createCourse);
    Task<CourseDto> UpdateCourseAsync(Guid courseId, UpdateCourse updateCourse);
    Task<CourseAggregateDto?> GetCourseContentAsync(Guid courseId);
    Task EnrollUserAsync(Guid courseId, Guid userId);
    Task DismissUserAsync(Guid enrollmentId);

    Task<double> GetCourseExamsProgressPercentAsync(Guid courseId, Guid userId);
    Task<double> GetCourseScoreProgressPercentAsync(Guid courseId, Guid userId);
    Task<double> GetUserCourseScoreAsync(Guid courseId, Guid userId);
}
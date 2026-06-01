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
    Task<CourseDto> CreateCurseAsync(CreateCourseDto createCourseDto);
    Task<CourseDto> UpdateCourseAsync(Guid courseId, UpdateCourseDto updateCourseDto);
    Task<CourseAggregateDto?> GetCourseContentAsync(Guid courseId);
    Task EnrollUserAsync(Guid courseId, Guid userId);
    Task DismissUserAsync(Guid enrollmentId);

    Task<double> GetCourseExamsProgressPercentAsync(Guid courseId, Guid userId, bool skipAccessCheck = false);
    Task<double> GetCourseScoreProgressPercentAsync(Guid courseId, Guid userId, bool skipAccessCheck = false);
    Task<double> GetUserCourseScoreAsync(Guid courseId, Guid userId, bool skipAccessCheck = false);

    Task<IReadOnlyList<AggregatedCourseStatsDto>> GetStatisticsForCoursesAsync(Guid userId, List<Guid> courseIds);
}
using DPBloom.Application.Base;
using DPBloom.Application.Course.Contracts;
using DPBloom.Core.Course;

namespace DPBloom.Application.Course;

public interface ICourseRepository : IRepository<CourseModel>
{
    Task<CourseAggregateDto?> GetCourseWithContentAsync(Guid courseId);
    Task<IReadOnlyList<CourseModel>> GetEnrolledCoursesByUserIdAsync(Guid userId);
    Task<bool> IsCourseAuthorAsync(Guid courseId, Guid userId);
    Task<Guid> GetTeacherIdByCourseAsync(Guid courseId);
}
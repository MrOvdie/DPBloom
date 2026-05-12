using DPBloom.Application.Base;
using DPBloom.Application.Course.Contracts;
using DPBloom.Core.Course;

namespace DPBloom.Application.Course;

public interface ICourseRepository : IRepository<CourseModel>
{
    Task<CourseAggregateDto?> GetCourseWithContentAsync(Guid courseId);
}
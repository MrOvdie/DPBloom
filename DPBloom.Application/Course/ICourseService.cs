using DPBloom.Application.Base;
using DPBloom.Application.Course.Contracts;
using DPBloom.Core.Course;

namespace DPBloom.Application.Course;

public interface ICourseService : ICrud<CourseDto>
{
    Task<IEnumerable<CourseDto>> GetCourseByNameAsync(string courseName);
    Task<IEnumerable<CourseDto>> GetCourseByAuthorAsync(Guid authorId);
    Task<CourseDto> CreateAsync(CreateCourse createCourse);
    Task<CourseDto> UpdateAsync(Guid courseId, UpdateCourse updateCourse);
    Task<CourseModel> GetEntityByIdAsync(Guid id);
}
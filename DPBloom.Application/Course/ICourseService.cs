using DPBloom.Application.Base;
using DPBloom.Application.Course.Contracts;
using DPBloom.Core.Course;

namespace DPBloom.Application.Course;

public interface ICourseService : ICrud<CourseDto>
{
    Task<IEnumerable<CourseDto>> GetCourseByNameAsync(string courseName);
    Task<IEnumerable<CourseDto>> GetCourseByAuthorAsync(string authorId);
    Task<CourseDto> CreateAsync(CreateCourse createCourse);
    Task<CourseDto> UpdateAsync(string courseId, UpdateCourse updateCourse);
    Task<CourseModel> GetEntityByIdAsync(string id);
}
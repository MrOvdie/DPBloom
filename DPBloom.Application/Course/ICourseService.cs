using DPBloom.Application.Base;
using DPBloom.Application.Course.Contracts;

namespace DPBloom.Application.Course;

public interface ICourseService : ICrud<CourseDto>
{
    Task<IEnumerable<CourseDto>> GetCourseByNameAsync(string courseName);
    Task<IEnumerable<CourseDto>> GetCourseByAuthorAsync(string authorId);
    Task<CourseDto> Create(CreateCourse createCourse);
    Task UpdateAsync(UpdateCourse updateCourse);
}
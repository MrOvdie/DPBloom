using DPBloom.Application.Course.Contracts;

namespace DPBloom.Application.Course;

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetCoursesListAsync();
    Task<CourseDto> GetCourseByIdAsync(string courseId);
    Task<IEnumerable<CourseDto>> GetCourseByNameAsync(string courseName);
    Task<IEnumerable<CourseDto>> GetCourseByAuthorAsync(string authorId);
    Task<CourseDto> Create(CreateCourse createCourse);
    Task UpdateAsync(UpdateCourse updateCourse);
    Task DeleteAsync(string courseId);
    Task RestoreAsync(string courseId);
}
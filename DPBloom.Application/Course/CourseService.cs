using AutoMapper;
using DPBloom.Application.Course.Contracts;
using DPBloom.Infrastructure.Course;

namespace DPBloom.Application.Course;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IMapper _mapper;

    public CourseService(ICourseRepository courseRepository, IMapper mapper)
    {
        _courseRepository = courseRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CourseDto>> GetAllAsync()
    {
        var courses = await _courseRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CourseDto>>(courses);
    }

    public async Task<CourseDto> GetByIdAsync(string id)
    {
        var course = await _courseRepository.GetByIdAsync(id);

        if (course is null)
        {
            throw new KeyNotFoundException($"Course with ID {id} not found");
        }
        
        return _mapper.Map<CourseDto>(course);
    }

    public async Task<IEnumerable<CourseDto>> GetCourseByNameAsync(string courseName)
    {
        var courses = await _courseRepository.GetAsync(predicate: l => l.Title == courseName);
        
        if (courses is null)
        {
            throw new KeyNotFoundException($"courses with Title {courseName} not found");
        }
        
        return _mapper.Map<IEnumerable<CourseDto>>(courses);
    }

    public async Task<IEnumerable<CourseDto>> GetCourseByAuthorAsync(string authorId)
    {
        var courses = await _courseRepository.GetAsync(predicate: l => l.AuthorId.Equals(authorId));
        
        if (courses is null)
        {
            throw new KeyNotFoundException($"courses from Author {authorId} not found");
        }
        
        return _mapper.Map<IEnumerable<CourseDto>>(courses);
    }

    public Task<CourseDto> Create(CreateCourse createCourse)
    {
        //TODO: try not to forget adding of Id prop :)
        throw new NotImplementedException();
    }

    public Task UpdateAsync(UpdateCourse updateCourse)
    {
        throw new NotImplementedException();
    }
    
    public async Task DeleteAsync(string id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        
        //TODO: Add validation!!!!

        await _courseRepository.DeleteAsync(course);
    }

    public async Task RestoreAsync(string id)
    {
        var course = await _courseRepository.GetByIdAsync(id);

        await _courseRepository.DeleteAsync(course);
    }
}
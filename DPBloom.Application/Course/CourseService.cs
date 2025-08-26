using AutoMapper;
using DPBloom.Application.Course.Contracts;
using DPBloom.Core.Course;
using DPBloom.Infrastructure.Course;
using FluentValidation;

namespace DPBloom.Application.Course;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCourse> _createCourseValidator;
    private readonly IValidator<UpdateCourse> _updateCourseValidator;

    public CourseService(ICourseRepository courseRepository, IMapper mapper,
        IValidator<CreateCourse> createCourseValidator, IValidator<UpdateCourse> updateCourseValidator)
    {
        _courseRepository = courseRepository;
        _mapper = mapper;
        _createCourseValidator = createCourseValidator;
        _updateCourseValidator = updateCourseValidator;
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
            throw new KeyNotFoundException($"Course with ID {id} not found");

        return _mapper.Map<CourseDto>(course);
    }

    public async Task<IEnumerable<CourseDto>> GetCourseByNameAsync(string courseName)
    {
        var courses = await _courseRepository.GetAsync(predicate: l => l.Title == courseName);

        if (courses is null)
            throw new KeyNotFoundException($"courses with Title {courseName} not found");

        return _mapper.Map<IEnumerable<CourseDto>>(courses);
    }

    public async Task<IEnumerable<CourseDto>> GetCourseByAuthorAsync(string authorId)
    {
        var courses = await _courseRepository.GetAsync(predicate: l => l.AuthorId.Equals(authorId));

        if (courses is null)
            throw new KeyNotFoundException($"courses from Author {authorId} not found");

        return _mapper.Map<IEnumerable<CourseDto>>(courses);
    }

    public async Task<CourseDto> CreateAsync(CreateCourse createCourse)
    {
        var validationResult = await _createCourseValidator.ValidateAsync(createCourse);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _courseRepository.ExistsAsync(c => c.Title == createCourse.Title && !c.IsDeleted ))
            throw new InvalidOperationException($"Course with name {createCourse.Title} already exists");

        var createCourseModel = _mapper.Map<CourseModel>(createCourse);
        createCourseModel.Id = Guid.NewGuid();
        createCourseModel.CreatedOn = createCourseModel.UpdatedOn = DateTime.UtcNow;

        var createdCourse = await _courseRepository.AddAsync(createCourseModel);

        return _mapper.Map<CourseDto>(createdCourse);
    }

    public async Task<CourseDto> UpdateAsync(string courseId, UpdateCourse updateCourse)
    {
        var validationResult = await _updateCourseValidator.ValidateAsync(updateCourse);
        
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        var existingCourse = await GetEntityByIdAsync(courseId);
        
        var updateCourseModel = _mapper.Map<CourseModel>(updateCourse);
        updateCourseModel.Id = existingCourse.Id;
        updateCourseModel.UpdatedOn = DateTime.UtcNow;
        
        var updatedCourse = await _courseRepository.UpdateAsync(updateCourseModel);
        
        return _mapper.Map<CourseDto>(updatedCourse);
    }

    public async Task<CourseDto> DeleteAsync(string id)
    {
        var course = await GetEntityByIdAsync(id);
        
        await _courseRepository.DeleteAsync(course);
        
        return _mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto> RestoreAsync(string id)
    {
        var course = await GetEntityByIdAsync(id);
        
        await _courseRepository.DeleteAsync(course);
        
        return _mapper.Map<CourseDto>(course);
    }

    public async Task<CourseModel> GetEntityByIdAsync(string id)
    {
        var lecture = await _courseRepository.GetByIdAsync(id);

        if (lecture is null)
            throw new KeyNotFoundException($"Course with ID {id} not found");

        return lecture;
    }
}
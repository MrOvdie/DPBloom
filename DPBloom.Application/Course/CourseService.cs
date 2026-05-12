using AutoMapper;
using DPBloom.Application.Auth;
using DPBloom.Application.Course.Contracts;
using DPBloom.Application.Topic;
using DPBloom.Application.User;
using DPBloom.Core.Course;
using DPBloom.Core.User;
using FluentValidation;
using UUIDNext;

namespace DPBloom.Application.Course;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCourse> _createCourseValidator;
    private readonly IValidator<UpdateCourse> _updateCourseValidator;
    private readonly ICurrentUserService _currentUserService;

    public CourseService(ICourseRepository courseRepository, IMapper mapper,
        IValidator<CreateCourse> createCourseValidator, IValidator<UpdateCourse> updateCourseValidator, ICurrentUserService currentUserService, ITopicRepository topicRepository, IEnrollmentRepository enrollmentRepository)
    {
        _courseRepository = courseRepository;
        _mapper = mapper;
        _createCourseValidator = createCourseValidator;
        _updateCourseValidator = updateCourseValidator;
        _currentUserService = currentUserService;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<IEnumerable<CourseDto>> GetAllAsync()
    {
        var courses = await _courseRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CourseDto>>(courses);
    }

    public async Task<CourseDto> GetByIdAsync(Guid id)
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

    public async Task<IEnumerable<CourseDto>> GetCourseByAuthorAsync(Guid authorId)
    {
        var courses = await _courseRepository.GetAsync(predicate: l => l.AuthorId.Equals(authorId));

        if (courses is null)
            throw new KeyNotFoundException($"courses from Author {authorId} not found");

        return _mapper.Map<IEnumerable<CourseDto>>(courses);
    }

    public async Task<CourseDto> CreateCurseAsync(CreateCourse createCourse)
    { //TODO: Figure out how to automatically add AuthorId to the course creation model
        var validationResult = await _createCourseValidator.ValidateAsync(createCourse);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _courseRepository.ExistsAsync(c => c.Title == createCourse.Title))
            throw new InvalidOperationException($"Course with name {createCourse.Title} already exists");

        var createCourseModel = _mapper.Map<CourseModel>(createCourse);
        createCourseModel.AuthorId = _currentUserService.GetUserId();
        createCourseModel.IsFinished = false;

        var createdCourse = await _courseRepository.AddAsync(createCourseModel);

        return _mapper.Map<CourseDto>(createdCourse);
    }

    public async Task<CourseDto> UpdateCourseAsync(Guid courseId, UpdateCourse updateCourse)
    {
        var validationResult = await _updateCourseValidator.ValidateAsync(updateCourse);
        
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        var existingCourse = await GetEntityByIdAsync(courseId);
        
        var updatedExistedCourseModel = _mapper.Map(updateCourse, existingCourse);
        
        updatedExistedCourseModel.UpdatedOn = DateTime.UtcNow;
        
        var updatedCourse = await _courseRepository.UpdateAsync(updatedExistedCourseModel);
        
        return _mapper.Map<CourseDto>(updatedCourse);
    }

    public async Task<CourseDto> DeleteAsync(Guid courseId)
    {
        var course = await GetEntityByIdAsync(courseId);
        
        await _courseRepository.DeleteAsync(courseId);
        
        return _mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto> RestoreAsync(Guid courseId)
    {
        var restoreCourse = await _courseRepository.RestoreAsync(courseId);
        
        return _mapper.Map<CourseDto>(restoreCourse);
    }

    private async Task<CourseModel> GetEntityByIdAsync(Guid id)
    {
        var lecture = await _courseRepository.GetByIdAsync(id);

        if (lecture is null)
            throw new KeyNotFoundException($"Lecture with ID {id} not found");

        return lecture;
    }

    public async Task<CourseAggregateDto?> GetCourseContentAsync(Guid courseId, bool bypassAccessCheck = false)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        
        if (course is null)
            throw new KeyNotFoundException("Course not found.");

        var userId = _currentUserService.GetUserId();
        
        if (!bypassAccessCheck)
        {
            var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, courseId);

            if (enrollment is null)
            {
                throw new UnauthorizedAccessException("You are not enrolled in this course.");
            }
        }
        
        var courseAggregate = await _courseRepository.GetCourseWithContentAsync(courseId);

        return courseAggregate;
    }
    
    public async Task EnrollUserAsync(Guid courseId, Guid userId)
    {
        _ = await GetEntityByIdAsync(courseId);

        var existingEnrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, courseId);

        if (existingEnrollment is not null)
        {
            throw new InvalidOperationException("User is already enrolled in this course.");
        }

        var createdEnrollment = new UserEnrollmentModel
        {
            Id = Uuid.NewDatabaseFriendly(Database.SqlServer),
            UserId = userId,
            CourseId = courseId,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow
        };

        await _enrollmentRepository.AddAsync(createdEnrollment);
    }

    public async Task DismissUserAsync(Guid enrollmentId)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
        
        if (enrollment is null)
            throw new KeyNotFoundException("Enrollment not found.");
        
        //await _enrollmentRepository.(enrollmentId);
    }
}
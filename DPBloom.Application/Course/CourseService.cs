using AutoMapper;
using DPBloom.Application.Auth;
using DPBloom.Application.Course.Contracts;
using DPBloom.Application.Course.Events;
using DPBloom.Application.Enrollment;
using DPBloom.Application.Enrollment.Contracts;
using DPBloom.Application.Exam;
using DPBloom.Core.Course;
using DPBloom.Core.User;
using FluentValidation;
using MediatR;

namespace DPBloom.Application.Course;

public class CourseService : ICourseService
{
    private readonly IAttemptResultRepository _attemptResultRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IValidator<CreateCourse> _createCourseValidator;
    private readonly IValidator<UpdateCourse> _updateCourseValidator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IExamRepository _examRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly IUserRepository _userRepository;

    public CourseService(ICourseRepository courseRepository, IMapper mapper,
        IValidator<CreateCourse> createCourseValidator, IValidator<UpdateCourse> updateCourseValidator,
        ICurrentUserService currentUserService,
        IEnrollmentRepository enrollmentRepository, IPublisher publisher, IUserRepository userRepository,
        IAttemptResultRepository attemptResultRepository, IExamRepository examRepository)
    {
        _courseRepository = courseRepository;
        _mapper = mapper;
        _createCourseValidator = createCourseValidator;
        _updateCourseValidator = updateCourseValidator;
        _currentUserService = currentUserService;
        _enrollmentRepository = enrollmentRepository;
        _publisher = publisher;
        _userRepository = userRepository;
        _attemptResultRepository = attemptResultRepository;
        _examRepository = examRepository;
    }

    public async Task<IReadOnlyList<CourseDto>> GetAllAsync()
    {
        var courses = await _courseRepository.GetAllAsync();
        return _mapper.Map<IReadOnlyList<CourseDto>>(courses);
    }

    public async Task<CourseDto> GetByIdAsync(Guid courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);

        if (course is null)
            throw new KeyNotFoundException($"Course with ID {courseId} not found");

        return _mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto> GetByIdWithAccessAsync(Guid courseId)
    {
        var userId = _currentUserService.GetUserId();
        
        await EnsureHasAccessToGenericCourseContent(courseId, userId);
        
        var course = await _courseRepository.GetByIdAsync(courseId);

        if (course is null)
            throw new KeyNotFoundException($"Course with ID {courseId} not found");

        return _mapper.Map<CourseDto>(course);
    }

    public async Task<IReadOnlyList<CourseDto>> GetCourseByNameAsync(string courseName)
    {
        var courses = await _courseRepository.GetAsync(predicate: l => l.Title == courseName);

        if (courses is null)
            throw new KeyNotFoundException($"courses with Title {courseName} not found");

        return _mapper.Map<IReadOnlyList<CourseDto>>(courses);
    }

    public async Task<IReadOnlyList<CourseDto>> GetCourseByAuthorAsync(Guid authorId)
    {
        var courses = await _courseRepository.GetAsync(predicate: l => l.AuthorId.Equals(authorId));

        if (courses is null)
            throw new KeyNotFoundException($"courses from Author {authorId} not found");

        return _mapper.Map<IReadOnlyList<CourseDto>>(courses);
    }

    public async Task<IReadOnlyList<CourseDto>> GetEnrolledCoursesAsync()
    {
        var userId = _currentUserService.GetUserId();
    
        var enrolledCourses = await _courseRepository.GetEnrolledCoursesByUserIdAsync(userId);

        var authoredCourse = await _courseRepository.GetAsync(predicate: l => l.AuthorId.Equals(userId));
    
        var allUserCourses = enrolledCourses
            .Concat(authoredCourse)
            .DistinctBy(c => c.Id)
            .ToList();
        
        return _mapper.Map<IReadOnlyList<CourseDto>>(allUserCourses);
    }

    public async Task<CourseDto> CreateCurseAsync(CreateCourse createCourse)
    {
        var validationResult = await _createCourseValidator.ValidateAsync(createCourse);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _courseRepository.ExistsAsync(c => c.Title == createCourse.Title))
            throw new InvalidOperationException($"Course with name {createCourse.Title} already exists");

        var createCourseModel = _mapper.Map<CourseModel>(createCourse);
        createCourseModel.AuthorId = createCourseModel.LastUpdaterId = _currentUserService.GetUserId();
        createCourseModel.IsFinished = false;

        var createdCourse = await _courseRepository.AddAsync(createCourseModel);

        return _mapper.Map<CourseDto>(createdCourse);
    }

    public async Task<CourseDto> UpdateCourseAsync(Guid courseId, UpdateCourse updateCourse)
    {
        var validationResult = await _updateCourseValidator.ValidateAsync(updateCourse);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
     
        await EnsureUserHasAccessToCourseAsync(courseId);
        
        var userId = _currentUserService.GetUserId();
        var isAdmin = _currentUserService.IsAdmin();
        
        if (!isAdmin)
        {
            var isAuthor = await _courseRepository.IsCourseAuthorAsync(courseId, userId);
            
            if (!isAuthor)
                throw new UnauthorizedAccessException("Only author or admins can update this course.");
        }

        var existingCourse = await GetEntityByIdAsync(courseId);

        var updatedExistedCourseModel = _mapper.Map(updateCourse, existingCourse);
        updatedExistedCourseModel.UpdatedOn = DateTime.UtcNow;
        updatedExistedCourseModel.LastUpdaterId = userId;

        var updatedCourse = await _courseRepository.UpdateAsync(updatedExistedCourseModel);

        return _mapper.Map<CourseDto>(updatedCourse);
    }

    public async Task<CourseDto> DeleteAsync(Guid courseId)
    {
        await EnsureUserHasAccessToCourseAsync(courseId);
        
        var course = await GetEntityByIdAsync(courseId);

        await _courseRepository.DeleteAsync(courseId);

        return _mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto> RestoreAsync(Guid courseId)
    {
        await EnsureUserHasAccessToCourseAsync(courseId);
        
        var restoreCourse = await _courseRepository.RestoreAsync(courseId);

        return _mapper.Map<CourseDto>(restoreCourse);
    }

    public async Task<CourseAggregateDto?> GetCourseContentAsync(Guid courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);

        if (course is null)
            throw new KeyNotFoundException("Course not found.");

        var userId = _currentUserService.GetUserId();

        await EnsureHasAccessToGenericCourseContent(courseId, userId);

        var courseAggregate = await _courseRepository.GetCourseWithContentAsync(courseId);

        return courseAggregate;
    }

    public async Task EnrollUserAsync(Guid courseId, Guid userId)
    {
        await EnsureUserHasAccessToCourseAsync(courseId);
        
        _ = await GetEntityByIdAsync(courseId);

        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        var enrollment = new CreateEnrollment
        {
            CourseId = courseId,
            UserId = userId,
            Status = EnrollmentStatusEnum.Active
        };

        await _publisher.Publish(new UserEnrolledEvent(enrollment));
    }

    public async Task DismissUserAsync(Guid enrollmentId)
    {
        var existingEnrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);

        if (existingEnrollment is null)
            throw new KeyNotFoundException("Enrollment not found.");

        await EnsureUserHasAccessToCourseAsync(existingEnrollment.CourseId);
        
        var updatedEnrolment = new UpdateEnrollment()
        {
            Status = EnrollmentStatusEnum.Active,
            FinalGrade = await GetUserCourseScoreAsync(existingEnrollment.CourseId, existingEnrollment.UserId)
        };

        await _publisher.Publish(new UserDismissedEvent(enrollmentId, updatedEnrolment));
    }

    private async Task<CourseModel> GetEntityByIdAsync(Guid id)
    {
        var lecture = await _courseRepository.GetByIdAsync(id);

        if (lecture is null)
            throw new KeyNotFoundException($"Lecture with ID {id} not found");

        return lecture;
    }

    public async Task<double> GetCourseExamsProgressPercentAsync(Guid courseId, Guid userId)
    {
        await EnsureUserHasAccessToPersonalCourseStatisticAsync(courseId, userId);
            
        var courseExams = await _examRepository.GetByCourseAsync(courseId);

        var totalCourseExamsCount = courseExams.Count;

        var userAttempts = await _attemptResultRepository
            .GetAsync(a => a.UserId.Equals(userId) && a.CourseId.Equals(courseId));

        var uniqueExamsPassed = userAttempts.GroupBy(ba => ba.ExamId)
            .Select(group => group.OrderByDescending(ba => ba.Score).First().Passed).Count();

        return (double)uniqueExamsPassed / totalCourseExamsCount * 100;
    }

    public async Task<double> GetCourseScoreProgressPercentAsync(Guid courseId, Guid userId)
    {
        await EnsureUserHasAccessToPersonalCourseStatisticAsync(courseId, userId);
        
        var userScore = await GetUserCourseScoreAsync(courseId, userId);

        var courseExams = await _examRepository.GetByCourseAsync(courseId);

        var maxScore = courseExams.Select(ce => ce.MaximumScore).Sum();

        return userScore / maxScore * 100;
    }

    public async Task<double> GetUserCourseScoreAsync(Guid courseId, Guid userId)
    {
        await EnsureUserHasAccessToPersonalCourseStatisticAsync(courseId, userId);
        
        var userAttempts = await _attemptResultRepository
            .GetAsync(a => a.UserId.Equals(userId) && a.CourseId.Equals(courseId));

        if (userAttempts is null)
            return 0;

        var uniqueBestAttmepts = userAttempts.GroupBy(ba => ba.ExamId)
            .Select(group => group.OrderByDescending(ba => ba.Score).First())
            .ToList();

        var userScore = uniqueBestAttmepts.Sum(a => a.Score);

        return userScore;
    }
    
    private async Task EnsureUserHasAccessToCourseAsync(Guid courseId)
    {
        var isAdmin = _currentUserService.IsAdmin();
        if (isAdmin) 
            return;

        var userId = _currentUserService.GetUserId();
        var isAuthor = await _courseRepository.IsCourseAuthorAsync(courseId, userId);
    
        if (!isAuthor)
        {
            var courseExists = await _courseRepository.ExistsAsync(l => l.Id.Equals(courseId));
            if (!courseExists)
                throw new KeyNotFoundException("Course not found.");
            
            throw new UnauthorizedAccessException("Only author or admins can modify this course.");
        }
    }
    
    private async Task EnsureStudentIsEnrolledAsync(Guid courseId, Guid studentId)
    {
        var isEnrolled = await _enrollmentRepository.ExistsAsync(studentId, courseId);
        if (!isEnrolled)
            throw new KeyNotFoundException("User is not enrolled in this course.");
    }
    
    private async Task EnsureHasAccessToGenericCourseContent(Guid courseId, Guid userId)
    {
        if (_currentUserService.IsAdmin()) return;

        var currentUserId = _currentUserService.GetUserId();

        var teacherId = await _courseRepository.GetTeacherIdByCourseAsync(courseId);
        if (teacherId == Guid.Empty) throw new KeyNotFoundException("Course not found.");

        if (teacherId == currentUserId) return;

        await EnsureStudentIsEnrolledAsync(courseId, currentUserId);
    }
    
    private async Task EnsureUserHasAccessToPersonalCourseStatisticAsync(Guid courseId, Guid userId)
    {
        if (_currentUserService.IsAdmin()) return;

        var currentUserId = _currentUserService.GetUserId();

        var teacherId = await _courseRepository.GetTeacherIdByCourseAsync(courseId);
        if (teacherId == Guid.Empty) throw new KeyNotFoundException("Course not found.");

        if (currentUserId != userId && currentUserId != teacherId)
            throw new UnauthorizedAccessException("Only the student, the course author, or admins can view this result.");

        await EnsureStudentIsEnrolledAsync(courseId, userId);
    }
}
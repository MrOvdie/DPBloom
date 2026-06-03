using AutoMapper;
using DPBloom.Application.Attempt;
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
using UUIDNext;

namespace DPBloom.Application.Course;

public class CourseService : ICourseService
{
    private readonly IAttemptResultRepository _attemptResultRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IValidator<CreateCourseDto> _createCourseValidator;
    private readonly IValidator<UpdateCourseDto> _updateCourseValidator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IExamRepository _examRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly IUserRepository _userRepository;

    public CourseService(ICourseRepository courseRepository, IMapper mapper,
        IValidator<CreateCourseDto> createCourseValidator, IValidator<UpdateCourseDto> updateCourseValidator,
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

    public async Task<IReadOnlyList<CourseDto>> GetUserCoursesAsync()
    {
        var userId = _currentUserService.GetUserId();

        var enrolledCourses = await _enrollmentRepository.GetAllByUserIdAsync(userId);
        var enrolledCoursesIds = enrolledCourses.Select(e => e.CourseId).Distinct().ToList();

        var courses = await _courseRepository.GetAsync(c =>
            (enrolledCoursesIds.Contains(c.Id) && c.IsPublished) || c.AuthorId == userId);

        var uniqueCourses = courses.DistinctBy(c => c.Id).ToList();

        return _mapper.Map<List<CourseDto>>(uniqueCourses);
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

    public async Task<CourseDto> CreateCurseAsync(CreateCourseDto createCourseDto)
    {
        var validationResult = await _createCourseValidator.ValidateAsync(createCourseDto);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _courseRepository.ExistsAsync(c => c.Title == createCourseDto.Title))
            throw new InvalidOperationException($"Course with name {createCourseDto.Title} already exists");

        var createCourseModel = _mapper.Map<CourseModel>(createCourseDto);
        createCourseModel.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);
        createCourseModel.CreatedOn = createCourseModel.UpdatedOn = DateTime.UtcNow;
        createCourseModel.AuthorId = createCourseModel.LastUpdaterId = _currentUserService.GetUserId();
        createCourseModel.IsFinished = false;

        var createdCourse = await _courseRepository.AddAsync(createCourseModel);

        return _mapper.Map<CourseDto>(createdCourse);
    }

    public async Task<CourseDto> UpdateCourseAsync(Guid courseId, UpdateCourseDto updateCourseDto)
    {
        var validationResult = await _updateCourseValidator.ValidateAsync(updateCourseDto);

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

        var updatedExistedCourseModel = _mapper.Map(updateCourseDto, existingCourse);
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

    public async Task EnrollUserAsync(Guid courseId, string userName)
    {
        await EnsureUserHasAccessToCourseAsync(courseId);

        _ = await GetEntityByIdAsync(courseId);

        var user = await _userRepository.GetUserByNameAsync(userName);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        var enrollment = new CreateEnrollmentDto
        {
            CourseId = courseId,
            UserId = user.Id,
            Status = EnrollmentStatusEnum.Active
        };

        await _publisher.Publish(new UserEnrolledEvent(enrollment));
    }

    public async Task EnrollMultipleUsersAsync(Guid courseId, List<string> userNames)
    {
        await EnsureUserHasAccessToCourseAsync(courseId);

        _ = await GetEntityByIdAsync(courseId);

        foreach (var userName in userNames)
        {
            var user = await _userRepository.GetUserByNameAsync(userName);

            if (user is null)
                throw new KeyNotFoundException($"User with name {userName} not found.");

            var enrollment = new CreateEnrollmentDto
            {
                CourseId = courseId,
                UserId = user.Id,
                Status = EnrollmentStatusEnum.Active
            };

            await _publisher.Publish(new UserEnrolledEvent(enrollment));
        }
    }

    public async Task EnrollGroupAsync(Guid courseId, string group)
    {
        await EnsureUserHasAccessToCourseAsync(courseId);

        _ = await GetEntityByIdAsync(courseId);

        var users = await _userRepository.GetUsersByGroupAsync(group);

        if (users is null || users.Count == 0)
            throw new KeyNotFoundException("Users not found.");

        foreach (var user in users)
        {
            var enrollment = new CreateEnrollmentDto
            {
                CourseId = courseId,
                UserId = user.Id,
                Status = EnrollmentStatusEnum.Active
            };

            await _publisher.Publish(new UserEnrolledEvent(enrollment));
        }
    }

    public async Task DismissUserAsync(Guid enrollmentId)
    {
        var existingEnrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);

        if (existingEnrollment is null)
            throw new KeyNotFoundException("Enrollment not found.");

        await EnsureUserHasAccessToCourseAsync(existingEnrollment.CourseId);

        var updatedEnrolment = new UpdateEnrollmentDto()
        {
            Status = EnrollmentStatusEnum.Active,
            FinalGrade = await GetUserCourseScoreAsync(existingEnrollment.CourseId, existingEnrollment.UserId)
        };

        await _publisher.Publish(new UserDismissedEvent(enrollmentId, updatedEnrolment));
    }

    private async Task<CourseModel> GetEntityByIdAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);

        if (course is null)
            throw new KeyNotFoundException($"Course with ID {id} not found");

        return course;
    }

    public async Task<double> GetCourseExamsProgressPercentAsync(Guid courseId, Guid userId,
        bool skipAccessCheck = false)
    {
        if (!skipAccessCheck)
            await EnsureUserHasAccessToPersonalCourseStatisticAsync(courseId, userId);

        var courseExams = await _examRepository.GetByCourseAsync(courseId);

        var totalCourseExamsCount = courseExams.Count;

        if (totalCourseExamsCount == 0) return 0;

        var userAttempts = await _attemptResultRepository
            .GetAsync(a => a.UserId.Equals(userId) && a.CourseId.Equals(courseId));

        var uniqueExamsPassed = userAttempts.GroupBy(ba => ba.ExamId)
            .Count(group => group.OrderByDescending(ba => ba.Score).First().Passed);

        return (double)uniqueExamsPassed / totalCourseExamsCount * 100;
    }

    public async Task<double> GetCourseScoreProgressPercentAsync(Guid courseId, Guid userId,
        bool skipAccessCheck = false)
    {
        if (!skipAccessCheck)
            await EnsureUserHasAccessToPersonalCourseStatisticAsync(courseId, userId);

        var userScore = await GetUserCourseScoreAsync(courseId, userId, skipAccessCheck);

        var courseExams = await _examRepository.GetByCourseAsync(courseId);

        var maxScore = courseExams.Select(ce => ce.MaximumScore).Sum();

        return maxScore > 0 ? (userScore / maxScore) * 100 : 0;
    }

    public async Task<double> GetUserCourseScoreAsync(Guid courseId, Guid userId, bool skipAccessCheck = false)
    {
        if (!skipAccessCheck)
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

    public async Task<IReadOnlyList<AggregatedCourseStatsDto>> GetStatisticsForCoursesAsync(Guid userId,
        List<Guid> courseIds)
    {
        var courseStatistics = new List<AggregatedCourseStatsDto>();

        if (courseIds is null || courseIds.Count == 0)
            return courseStatistics;

        var enrolledCourseIds = await _enrollmentRepository.GetEnrolledCourseIdsAsync(userId, courseIds);

        foreach (var courseId in enrolledCourseIds)
        {
            var score = await GetUserCourseScoreAsync(courseId, userId, skipAccessCheck: true);
            var testCompletion = await GetCourseExamsProgressPercentAsync(courseId, userId, skipAccessCheck: true);
            var courseCompletion = await GetCourseScoreProgressPercentAsync(courseId, userId, skipAccessCheck: true);

            courseStatistics.Add(new AggregatedCourseStatsDto()
            {
                CourseId = courseId,
                Score = Math.Round(score, 2),
                ExamCompletion = Math.Round(testCompletion, 2),
                CourseCompletion = Math.Round(courseCompletion, 2),
            });
        }

        return courseStatistics;
    }

    private async Task EnsureUserHasAccessToCourseAsync(Guid courseId)
    {
        var courseExists = await _courseRepository.ExistsAsync(l => l.Id.Equals(courseId));
        if (!courseExists)
            throw new KeyNotFoundException("Course not found.");

        var isAdmin = _currentUserService.IsAdmin();
        if (isAdmin)
            return;

        var userId = _currentUserService.GetUserId();
        var isAuthor = await _courseRepository.IsCourseAuthorAsync(courseId, userId);

        if (!isAuthor)
            throw new UnauthorizedAccessException("Only author or admins can modify this course.");
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
            throw new UnauthorizedAccessException(
                "Only the student, the course author, or admins can view this result.");

        await EnsureStudentIsEnrolledAsync(courseId, userId);
    }
}
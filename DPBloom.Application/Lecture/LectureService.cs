using AutoMapper;
using DPBloom.Application.Auth;
using DPBloom.Application.Enrollment;
using DPBloom.Application.Lecture.Contracts;
using DPBloom.Application.Topic;
using DPBloom.Core.Lecture;
using FluentValidation;
using UUIDNext;

namespace DPBloom.Application.Lecture;

public class LectureService : ILectureService
{
    private readonly ILectureRepository _lectureRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateLectureDto> _createValidator;
    private readonly IValidator<UpdateLectureDto> _updateValidator;
    private readonly ICurrentUserService _currentUserService;


    public LectureService(ILectureRepository lectureRepository, IMapper mapper,
        IValidator<CreateLectureDto> createValidator, IValidator<UpdateLectureDto> updateValidator,
        ICurrentUserService currentUserService, IEnrollmentRepository enrollmentRepository,
        ITopicRepository topicRepository)
    {
        _lectureRepository = lectureRepository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUserService = currentUserService;
        _enrollmentRepository = enrollmentRepository;
        _topicRepository = topicRepository;
    }

    public async Task<IReadOnlyList<LectureDto>> GetAllAsync()
    {
        var lectures = await _lectureRepository.GetAllAsync();
        return _mapper.Map<IReadOnlyList<LectureDto>>(lectures);
    }

    public async Task<LectureDto> GetByIdAsync(Guid courseId)
    {
        var lecture = await _lectureRepository.GetByIdAsync(courseId);

        if (lecture is null)
            throw new KeyNotFoundException($"Lecture with ID {courseId} not found");

        return _mapper.Map<LectureDto>(lecture);
    }

    public async Task<LectureDetailsDto> GetDetailsByIdWithAccessAsync(Guid lectureId)
    {
        var courseId = await _lectureRepository.GetCourseIdByLectureIdAsync(lectureId);
        if (courseId is null)
            throw new KeyNotFoundException($"Can't find CourseId in lecture {lectureId}");

        await EnsureHasAccessToGenericLectureContent(lectureId);

        var lecture = await _lectureRepository.GetByIdAsync(lectureId);

        return _mapper.Map<LectureDetailsDto>(lecture);
    }
    
    public async Task<IReadOnlyList<LectureDto>> GetLectureByNameAsync(string lectureName)
    {
        var lectures = await _lectureRepository.GetAsync(predicate: l => l.Title == lectureName);
        if (lectures is null)
            throw new KeyNotFoundException($"Lectures with Title {lectureName} not found");

        return _mapper.Map<List<LectureDto>>(lectures);
    }

    public async Task<IReadOnlyList<LectureDto>> GetLecturesByCourseAsync(Guid courseId)
    {
        var userId = _currentUserService.GetUserId();

        await EnsureStudentIsEnrolledToCourseAsync(courseId, userId);
        
        var lectures = await _lectureRepository.GetByCourseAsync(courseId);

        return _mapper.Map<List<LectureDto>>(lectures);
    }

    public async Task<IReadOnlyList<LectureDto>> GetLecturesByTopicAsync(Guid topicId)
    {
        var userId = _currentUserService.GetUserId();

        var courseId = await _topicRepository.GetCourseIdByTopicIdAsync(topicId);
        if (courseId is null)
            throw new KeyNotFoundException($"Can't find CourseId in lecture {topicId}");

        await EnsureStudentIsEnrolledToCourseAsync(courseId.Value, userId);

        var lecture = await _lectureRepository.GetByCourseAsync(courseId.Value);

        return _mapper.Map<List<LectureDto>>(lecture);
    }

    public async Task<IReadOnlyList<LectureDto>> GetLectureByAuthorAsync(Guid authorId)
    {
        var lectures = await _lectureRepository.GetAsync(predicate: l => l.AuthorId.Equals(authorId));
        if (lectures is null)
            throw new KeyNotFoundException($"Lectures from Author {authorId} not found");

        return _mapper.Map<List<LectureDto>>(lectures);
    }

    public async Task<LectureDetailsDto> CreateAsync(Guid courseId, CreateLectureDto createLectureDto)
    {
        var validationResult = await _createValidator.ValidateAsync(createLectureDto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _lectureRepository.ExistsAsync(l =>
                l.Title == createLectureDto.Title && l.CourseId.Equals(courseId)))
            throw new InvalidOperationException(
                $"Lecture with name {createLectureDto.Title} already exists in this course");

        var createLectureModel = _mapper.Map<LectureModel>(createLectureDto);
        createLectureModel.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);
        createLectureModel.CreatedOn = createLectureModel.UpdatedOn = DateTime.UtcNow;
        createLectureModel.AuthorId = createLectureModel.LastUpdaterId = _currentUserService.GetUserId();
        createLectureModel.CourseId = courseId;

        var createdLecture = await _lectureRepository.AddAsync(createLectureModel);

        return _mapper.Map<LectureDetailsDto>(createdLecture);
    }

    public async Task<LectureDetailsDto> UpdateAsync(Guid lectureId, UpdateLectureDto updateLectureDto)
    {
        var validationResult = await _updateValidator.ValidateAsync(updateLectureDto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        await EnsureUserHasAccessToLectureModifyingAsync(lectureId);

        var existingLecture = await GetEntityByIdAsync(lectureId);

        updateLectureDto.CourseId ??= existingLecture.CourseId;

        updateLectureDto.TopicId ??= existingLecture.TopicId;

        var updatedExistedLectureModel = _mapper.Map(updateLectureDto, existingLecture);
        updatedExistedLectureModel.UpdatedOn = DateTime.UtcNow;
        updatedExistedLectureModel.LastUpdaterId = _currentUserService.GetUserId();

        var updatedLecture = await _lectureRepository.UpdateAsync(updatedExistedLectureModel);

        return _mapper.Map<LectureDetailsDto>(updatedLecture);
    }

    public async Task<LectureDto> DeleteAsync(Guid lectureId)
    {
        await EnsureUserHasAccessToLectureModifyingAsync(lectureId);

        var lecture = await GetEntityByIdAsync(lectureId);

        await _lectureRepository.DeleteAsync(lectureId);

        return _mapper.Map<LectureDto>(lecture);
    }

    public async Task<LectureDto> RestoreAsync(Guid lectureId)
    {
        await EnsureUserHasAccessToLectureModifyingAsync(lectureId);

        var restoredLecture = await _lectureRepository.RestoreAsync(lectureId);

        return _mapper.Map<LectureDto>(restoredLecture);
    }

    public async Task<LectureModel> GetEntityByIdAsync(Guid id)
    {
        var lecture = await _lectureRepository.GetByIdAsync(id);
        if (lecture is null)
            throw new KeyNotFoundException($"Lecture with ID {id} not found");

        return lecture;
    }
    
    private async Task EnsureStudentIsEnrolledToCourseAsync(Guid courseId, Guid studentId)
    {
        var isEnrolled = await _enrollmentRepository.ExistsAsync(studentId, courseId);
        if (!isEnrolled)
            throw new KeyNotFoundException("User is not enrolled in this course.");
    }

    private async Task EnsureStudentIsEnrolledAsync(Guid lectureId, Guid studentId)
    {
        var courseId = await _lectureRepository.GetCourseIdByLectureIdAsync(lectureId);
        if (courseId is null) 
            throw new KeyNotFoundException("Course not found.");
        
        var isEnrolled = await _enrollmentRepository.ExistsAsync(studentId, courseId.Value);
        if (!isEnrolled)
            throw new KeyNotFoundException("User is not enrolled in this course.");
    }

    private async Task EnsureHasAccessToGenericLectureContent(Guid lectureId)
    {
        if (_currentUserService.IsAdmin()) return;

        var currentUserId = _currentUserService.GetUserId();

        var teacherId = await _lectureRepository.IsLectureAuthorAsync(lectureId, currentUserId);
        if (teacherId) return;

        await EnsureStudentIsEnrolledAsync(lectureId, currentUserId);
    }
    
    private async Task EnsureUserHasAccessToLectureModifyingAsync(Guid lectureId)
    {
        var isAdmin = _currentUserService.IsAdmin();
        if (isAdmin)
            return;

        var userId = _currentUserService.GetUserId();
        
        var isAuthor = await _lectureRepository.IsLectureAuthorAsync(lectureId, userId);
        if (!isAuthor)
        {
            var lectureExists = await _lectureRepository.ExistsAsync(l => l.Id.Equals(lectureId));
            if (!lectureExists)
                throw new KeyNotFoundException("Lecture not found.");

            throw new UnauthorizedAccessException("Only author or admins can modify this lecture.");
        }
    }
}
using AutoMapper;
using DPBloom.Application.Auth;
using DPBloom.Application.Enrollment;
using DPBloom.Application.Topic.Contracts;
using DPBloom.Core.Topic;
using FluentValidation;
using UUIDNext;

namespace DPBloom.Application.Topic;

public class TopicService : ITopicService
{
    private readonly ITopicRepository _topicRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateTopicDto> _createTopicValidator;
    private readonly IValidator<UpdateTopicDto> _updateTopicValidator;
    private readonly ICurrentUserService _currentUserService;

    public TopicService(ITopicRepository topicRepository, IMapper mapper, IValidator<CreateTopicDto> createTopicValidator,
        IValidator<UpdateTopicDto> updateTopicValidator, ICurrentUserService currentUserService, IEnrollmentRepository enrollmentRepository)
    {
        _topicRepository = topicRepository;
        _mapper = mapper;
        _createTopicValidator = createTopicValidator;
        _updateTopicValidator = updateTopicValidator;
        _currentUserService = currentUserService;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<IReadOnlyList<TopicDto>> GetAllAsync()
    {
        var topics = await _topicRepository.GetAllAsync();
        return _mapper.Map<IReadOnlyList<TopicDto>>(topics);
    }

    public async Task<TopicDto> GetByIdAsync(Guid topicId)
    {
        var topic = await _topicRepository.GetByIdAsync(topicId);

        if (topic is null)
            throw new KeyNotFoundException($"Topic with ID {topicId} not found");

        return _mapper.Map<TopicDto>(topic);
    }

    public async Task<TopicDto> GetByIdWithAccessAsync(Guid topicId)
    {
        var courseId = await _topicRepository.GetCourseIdByTopicIdAsync(topicId);

        if (courseId is null)
            throw new KeyNotFoundException($"Can't find CourseId in topic {topicId}");

        await EnsureHasAccessToGenericTopicContent(topicId);

        var topic = await _topicRepository.GetByIdAsync(topicId);

        return _mapper.Map<TopicDto>(topic);
    }

    public async Task<IReadOnlyList<TopicDto>> GetTopicByNameAsync(string topicName)
    {
        var topics = await _topicRepository.GetAsync(predicate: t => t.Title == topicName);
        if (topics is null)
            throw new KeyNotFoundException($"Topic with Title {topicName} not found");

        return _mapper.Map<List<TopicDto>>(topics);
    }

    public async Task<IReadOnlyList<TopicDto>> GetTopicsByAuthorAsync(Guid authorId)
    {
        var topics = await _topicRepository.GetAsync(predicate: t => t.AuthorId.Equals(authorId));

        if (topics is null)
            throw new KeyNotFoundException($"Topics from Author {authorId} not found");

        return _mapper.Map<List<TopicDto>>(topics);
    }
    
    public async Task<IReadOnlyList<TopicDto>> GetTopicsByCourseAsync(Guid courseId)
    {
        var user = _currentUserService.GetUserId();
        
        await EnsureUserHasAccessToTopicModifyingAsync(courseId);
        
        var topics = await _topicRepository.GetAsync(predicate: t => t.CourseId.Equals(courseId));
        if (topics is null)
            throw new KeyNotFoundException($"Topics in course {courseId} not found");
        
        return _mapper.Map<List<TopicDto>>(topics);
    }

    public async Task<TopicDto> CreateAsync(Guid courseId, CreateTopicDto createTopicDto)
    {
        var validationResult = await _createTopicValidator.ValidateAsync(createTopicDto);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var createTopicModel = _mapper.Map<TopicModel>(createTopicDto);
        createTopicModel.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);
        createTopicModel.CreatedOn = createTopicModel.UpdatedOn = DateTime.UtcNow;
        createTopicModel.AuthorId = createTopicModel.LastUpdaterId = _currentUserService.GetUserId();
        createTopicModel.CourseId = courseId;

        if (await _topicRepository.ExistsAsync(tc =>
                tc.Title == createTopicModel.Title && tc.CourseId.Equals(courseId)))
            throw new InvalidOperationException(
                $"Topic with name {createTopicModel.Title} already exists in this course");

        var createdTopic = await _topicRepository.AddAsync(createTopicModel);

        return _mapper.Map<TopicDto>(createdTopic);
    }

    public async Task<TopicDto> UpdateAsync(Guid topicId, UpdateTopicDto updateTopicDto)
    {
        
        var validationResult = await _updateTopicValidator.ValidateAsync(updateTopicDto);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        await EnsureUserHasAccessToTopicAsync(topicId);
        
        var existingTopic = await GetEntityByIdAsync(topicId);
        
        updateTopicDto.CourseId ??= existingTopic.CourseId;

        var updatedExistedTopicModel = _mapper.Map(updateTopicDto, existingTopic);
        updatedExistedTopicModel.UpdatedOn = DateTime.UtcNow;
        updatedExistedTopicModel.LastUpdaterId = _currentUserService.GetUserId();

        var updatedCourse = await _topicRepository.UpdateAsync(updatedExistedTopicModel);

        return _mapper.Map<TopicDto>(updatedCourse);
    }

    public async Task<TopicDto> DeleteAsync(Guid topicId)
    {
        await EnsureUserHasAccessToTopicModifyingAsync(topicId);
        
        var topic = await GetEntityByIdAsync(topicId);

        await _topicRepository.DeleteAsync(topicId);

        return _mapper.Map<TopicDto>(topic);
    }

    public async Task<TopicDto> RestoreAsync(Guid topicId)
    {
        await EnsureUserHasAccessToTopicModifyingAsync(topicId);
        
        var restoredTopic = await _topicRepository.RestoreAsync(topicId);

        return _mapper.Map<TopicDto>(restoredTopic);
    }

    public async Task<TopicModel> GetEntityByIdAsync(Guid id)
    {
        var topic = await _topicRepository.GetByIdAsync(id);

        if (topic is null)
            throw new KeyNotFoundException($"Topic with ID {id} not found");

        return topic;
    }
    
    private async Task EnsureUserHasAccessToTopicAsync(Guid topicId)
    {
        var isAdmin = _currentUserService.IsAdmin();
        if (isAdmin) 
            return;

        var userId = _currentUserService.GetUserId();
        var isAuthor = await _topicRepository.IsTopicAuthorAsync(topicId, userId);
    
        if (!isAuthor)
        {
            var topicExists = await _topicRepository.ExistsAsync(l => l.Id.Equals(topicId));
            if (!topicExists)
                throw new KeyNotFoundException("Topic not found.");
            
            throw new UnauthorizedAccessException("Only author or admins can modify this topic.");
        }
    }
    
    private async Task EnsureStudentIsEnrolledAsync(Guid topicId, Guid userId)
    {
        var courseId = await _topicRepository.GetCourseIdByTopicIdAsync(topicId);
        if (courseId is null) 
            throw new KeyNotFoundException("Course not found.");
        
        var isEnrolled = await _enrollmentRepository.ExistsAsync(userId, courseId.Value);
        if (!isEnrolled)
            throw new KeyNotFoundException("User is not enrolled in this course.");
    }

    private async Task EnsureHasAccessToGenericTopicContent(Guid topicId)
    {
        if (_currentUserService.IsAdmin()) return;

        var currentUserId = _currentUserService.GetUserId();

        var teacherId = await _topicRepository.IsTopicAuthorAsync(topicId, currentUserId);

        if (teacherId) return;

        await EnsureStudentIsEnrolledAsync(topicId, currentUserId);
    }
    
    private async Task EnsureUserHasAccessToTopicModifyingAsync(Guid lectureId)
    {
        var isAdmin = _currentUserService.IsAdmin();
        if (isAdmin)
            return;

        var userId = _currentUserService.GetUserId();
        var isAuthor = await _topicRepository.IsTopicAuthorAsync(lectureId, userId);

        if (!isAuthor)
        {
            var topicExists = await _topicRepository.ExistsAsync(l => l.Id.Equals(lectureId));
            if (!topicExists)
                throw new KeyNotFoundException("Topic not found.");

            throw new UnauthorizedAccessException("Only author or admins can modify this topic.");
        }
    }
}
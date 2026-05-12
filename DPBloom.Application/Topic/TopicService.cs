using AutoMapper;
using DPBloom.Application.Auth;
using DPBloom.Application.Topic.Contracts;
using DPBloom.Core.Topic;
using FluentValidation;

namespace DPBloom.Application.Topic;

public class TopicService : ITopicService
{
    private readonly ITopicRepository _topicRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateTopic> _createTopicValidator;
    private readonly IValidator<UpdateTopic> _updateTopicValidator;
    private readonly ICurrentUserService _currentUserService;

    public TopicService(ITopicRepository topicRepository, IMapper mapper, IValidator<CreateTopic> createTopicValidator,
        IValidator<UpdateTopic> updateTopicValidator, ICurrentUserService currentUserService)
    {
        _topicRepository = topicRepository;
        _mapper = mapper;
        _createTopicValidator = createTopicValidator;
        _updateTopicValidator = updateTopicValidator;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<TopicDto>> GetAllAsync()
    {
        var topics = await _topicRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<TopicDto>>(topics);
    }

    public async Task<TopicDto> GetByIdAsync(Guid topicId)
    {
        var topic = await _topicRepository.GetByIdAsync(topicId);

        if (topic is null)
            throw new KeyNotFoundException($"Topic with ID {topicId} not found");

        return _mapper.Map<TopicDto>(topic);
    }

    public async Task<IEnumerable<TopicDto>> GetTopicByNameAsync(string topicName)
    {
        var topics = await _topicRepository.GetAsync(predicate: t => t.Title == topicName);
        if (topics is null)
            throw new KeyNotFoundException($"Topic with Title {topicName} not found");

        return _mapper.Map<IEnumerable<TopicDto>>(topics);
    }

    public async Task<IEnumerable<TopicDto>> GetTopicByAuthorAsync(Guid authorId)
    {
        var topics = await _topicRepository.GetAsync(predicate: t => t.AuthorId.Equals(authorId));

        if (topics is null)
            throw new KeyNotFoundException($"Topics from Author {authorId} not found");

        return _mapper.Map<IEnumerable<TopicDto>>(topics);
    }

    public async Task<TopicDto> CreateAsync(Guid courseId, CreateTopic createTopic)
    {
        var validationResult = await _createTopicValidator.ValidateAsync(createTopic);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var createTopicModel = _mapper.Map<TopicModel>(createTopic);
        createTopicModel.AuthorId = _currentUserService.GetUserId();
        createTopicModel.CourseId = courseId;

        if (await _topicRepository.ExistsAsync(tc =>
                tc.Title == createTopicModel.Title && tc.CourseId.Equals(courseId)))
            throw new InvalidOperationException(
                $"Topic with name {createTopicModel.Title} already exists in this course");

        var createdTopic = await _topicRepository.AddAsync(createTopicModel);

        return _mapper.Map<TopicDto>(createdTopic);
    }

    public async Task<TopicDto> UpdateAsync(Guid topicId, UpdateTopic updateTopic)
    {
        var validationResult = await _updateTopicValidator.ValidateAsync(updateTopic);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingTopic = await GetEntityByIdAsync(topicId);
        
        updateTopic.CourseId ??= existingTopic.CourseId;

        var updatedExistedTopicModel = _mapper.Map(updateTopic, existingTopic);
        
        updatedExistedTopicModel.UpdatedOn = DateTime.UtcNow;

        var updatedCourse = await _topicRepository.UpdateAsync(updatedExistedTopicModel);

        return _mapper.Map<TopicDto>(updatedCourse);
    }

    public async Task<TopicDto> DeleteAsync(Guid topicId)
    {
        var topic = await GetEntityByIdAsync(topicId);

        await _topicRepository.DeleteAsync(topicId);

        return _mapper.Map<TopicDto>(topic);
    }

    public async Task<TopicDto> RestoreAsync(Guid topicId)
    {
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
}
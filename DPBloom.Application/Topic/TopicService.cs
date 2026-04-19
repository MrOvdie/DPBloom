using AutoMapper;
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

    public TopicService(ITopicRepository topicRepository, IMapper mapper, IValidator<CreateTopic> createTopicValidator,
        IValidator<UpdateTopic> updateTopicValidator)
    {
        _topicRepository = topicRepository;
        _mapper = mapper;
        _createTopicValidator = createTopicValidator;
        _updateTopicValidator = updateTopicValidator;
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

    public async Task<TopicDto> CreateAsync(CreateTopic createTopic)
    {
        var validationResult = await _createTopicValidator.ValidateAsync(createTopic);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var createTopicModel = _mapper.Map<TopicModel>(createTopic);
        createTopicModel.Id = Guid.NewGuid();
        createTopicModel.CreatedOn = createTopicModel.UpdatedOn = DateTime.UtcNow;

        if (await _topicRepository.ExistsAsync(tc =>
                tc.Title == createTopicModel.Title && tc.CourseId.Equals(createTopic.CourseId) /*&& !tc.IsDeleted*/))
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

        var updateTopicModel = _mapper.Map<TopicModel>(updateTopic);
        updateTopicModel.Id = existingTopic.Id;
        updateTopicModel.UpdatedOn = DateTime.UtcNow;

        var updatedCourse = await _topicRepository.UpdateAsync(updateTopicModel);

        return _mapper.Map<TopicDto>(updatedCourse);
    }

    public async Task<TopicDto> DeleteAsync(Guid topicId)
    {
        var topic = await GetEntityByIdAsync(topicId);

        await _topicRepository.DeleteAsync(topic);

        return _mapper.Map<TopicDto>(topic);
    }

    public async Task<TopicDto> RestoreAsync(Guid topicId)
    {
        var topic = await GetEntityByIdAsync(topicId);

        await _topicRepository.RestoreAsync(topic);

        return _mapper.Map<TopicDto>(topic);
    }

    public async Task<TopicModel> GetEntityByIdAsync(Guid id)
    {
        var topic = await _topicRepository.GetByIdAsync(id);

        if (topic is null)
            throw new KeyNotFoundException($"Topic with ID {id} not found");

        return topic;
    }
}
using AutoMapper;
using DPBloom.Application.Topic.Contracts;
using DPBloom.Core.Topic;
using DPBloom.Infrastructure.Topic;

namespace DPBloom.Application.Topic;

public class TopicService : ITopicService
{
    private readonly ITopicRepository _topicRepository;
    private readonly IMapper _mapper;

    public TopicService(ITopicRepository topicRepository, IMapper mapper)
    {
        _topicRepository = topicRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TopicDto>> GetTopicsListAsync()
    {
        var topics = await _topicRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<TopicDto>>(topics);
    }

    public async Task<TopicDto> GetTopicByIdAsync(string topicId)
    {
        var topic = await _topicRepository.GetByIdAsync(topicId);

        if (topic is null)
        {
            throw new KeyNotFoundException($"Topic with ID {topicId} not found");
        }

        return _mapper.Map<TopicDto>(topic);
    }

    public async Task<IEnumerable<TopicDto>> GetTopicByNameAsync(string topicName)
    {
        var topics = await _topicRepository.GetAsync(predicate: t => t.Title == topicName);
        if (topics is null)
        {
            throw new KeyNotFoundException($"Topic with Title {topicName} not found");
        }

        return _mapper.Map<IEnumerable<TopicDto>>(topics);
    }

    public async Task<IEnumerable<TopicDto>> GetTopicByAuthorAsync(string authorId)
    {
        var topics = await _topicRepository.GetAsync(predicate: t => t.AuthorId.Equals(authorId));

        if (topics is null)
        {
            throw new KeyNotFoundException($"Topics from Author {authorId} not found");
        }

        return _mapper.Map<IEnumerable<TopicDto>>(topics);
    }

    public async Task<TopicDto> CreateAsync(CreateTopic createTopic)
    {
        if (createTopic is null)
            throw new ArgumentNullException(nameof(createTopic));
        
        var topicCreate =  _mapper.Map<TopicModel>(createTopic);

        var isDuplicate = await _topicRepository.ExistsAsync(tc => tc.Title == topicCreate.Title);
        if (isDuplicate)
            throw new InvalidOperationException($"Topic with name {topicCreate.Title} already exists");

        var createdTopic = await _topicRepository.AddAsync(topicCreate);
        
        return _mapper.Map<TopicDto>(createdTopic);
    }

    public async Task UpdateAsync(UpdateTopic updateTopic)
    {
        //TODO: write validation update entity and mapper profile for that
        var topicUpdate = _mapper.Map<TopicModel>(updateTopic);
        await _topicRepository.UpdateAsync(topicUpdate);
    }

    public async Task DeleteAsync(string topicId)
    {
        var topic = await _topicRepository.GetByIdAsync(topicId);

        await _topicRepository.DeleteAsync(topic);
    }

    public async Task RestoreAsync(string topicId)
    {
        var topic = await _topicRepository.GetByIdAsync(topicId);

        await _topicRepository.RestoreAsync(topic);
    }
}
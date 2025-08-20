using AutoMapper;
using DPBloom.Application.Lecture.Contracts;
using DPBloom.Core;
using DPBloom.Core.Lecture;
using DPBloom.Infrastructure.Lecture;

namespace DPBloom.Application.Lecture;

public class LectureService : ILectureService
{
    private readonly ILectureRepository _lectureRepository;
    private readonly IMapper _mapper;

    public LectureService(ILectureRepository lectureRepository, IMapper mapper)
    {
        _lectureRepository = lectureRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LectureDto>> GetAllAsync()
    {
        var lectures = await _lectureRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<LectureDto>>(lectures);
    }

    public async Task<LectureDto> GetByIdAsync(string lectureId)
    {
        var lecture = await _lectureRepository.GetByIdAsync(lectureId);

        if (lecture is null)
        {
            throw new KeyNotFoundException($"Lecture with ID {lectureId} not found");
        }
        
        return _mapper.Map<LectureDto>(lecture);
    }

    public async Task<IEnumerable<LectureDto>> GetLectureByNameAsync(string lectureName)
    {
        var lectures = await _lectureRepository.GetAsync(predicate: l => l.Title == lectureName);
        
        if (lectures is null)
        {
            throw new KeyNotFoundException($"Lectures with Title {lectureName} not found");
        }
        
        return _mapper.Map<IEnumerable<LectureDto>>(lectures);
    }

    public async Task<IEnumerable<LectureDto>> GetLecturesByCourseAsync(string courseId)
    {
        var lectures = await _lectureRepository.GetByCourseAsync(courseId);
        
        if (lectures is null)
        {
            throw new KeyNotFoundException($"Lectures with CourseId {courseId} not found");
        }
        
        return _mapper.Map<IEnumerable<LectureDto>>(lectures);
    }

    public async Task<IEnumerable<LectureDto>> GetLecturesByTopicAsync(string topicId)
    { //TODO: add proper topic handler, not just an id
        var lectures = await _lectureRepository.GetByTopicAsync(topicId);
        
        if (lectures is null)
        {
            throw new KeyNotFoundException($"Lectures with TopicId {topicId} not found");
        }
        
        return _mapper.Map<IEnumerable<LectureDto>>(lectures);
    }

    public async Task<IEnumerable<LectureDto>> GetLectureByAuthorAsync(string authorId)
    {
        var lectures = await _lectureRepository.GetAsync(predicate: l => l.AuthorId.Equals(authorId));
        
        if (lectures is null)
        {
            throw new KeyNotFoundException($"Lectures from Author {authorId} not found");
        }
        
        return _mapper.Map<IEnumerable<LectureDto>>(lectures);
    }

    public Task<LectureDto> Create(CreateLecture createModel)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(UpdateLecture updateLecture)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(string lectureId)
    {
        var lecture = await _lectureRepository.GetByIdAsync(lectureId);

        await _lectureRepository.DeleteAsync(lecture);
    }

    public async Task RestoreAsync(string lectureId)
    {
        var lecture = await _lectureRepository.GetByIdAsync(lectureId);

        await _lectureRepository.RestoreAsync(lecture);
    }
}
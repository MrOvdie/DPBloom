using AutoMapper;
using DPBloom.Application.Lecture.Contracts;
using DPBloom.Core.Lecture;
using FluentValidation;

namespace DPBloom.Application.Lecture;

public class LectureService : ILectureService
{
    private readonly ILectureRepository _lectureRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateLecture> _createValidator;
    private readonly IValidator<UpdateLecture> _updateValidator;

    public LectureService(ILectureRepository lectureRepository, IMapper mapper,
        IValidator<CreateLecture> createValidator, IValidator<UpdateLecture> updateValidator)
    {
        _lectureRepository = lectureRepository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<LectureDto>> GetAllAsync()
    {
        var lectures = await _lectureRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<LectureDto>>(lectures);
    }

    public async Task<LectureDto> GetByIdAsync(Guid id)
    {
        var lecture = await _lectureRepository.GetByIdAsync(id);

        if (lecture is null)
            throw new KeyNotFoundException($"Lecture with ID {id} not found");
        
        return _mapper.Map<LectureDto>(lecture);
    }

    public async Task<IEnumerable<LectureDto>> GetLectureByNameAsync(string lectureName)
    {
        var lectures = await _lectureRepository.GetAsync(predicate: l => l.Title == lectureName);

        if (lectures is null)
            throw new KeyNotFoundException($"Lectures with Title {lectureName} not found");

        return _mapper.Map<IEnumerable<LectureDto>>(lectures);
    }

    public async Task<IEnumerable<LectureDto>> GetLecturesByCourseAsync(Guid courseId)
    {
        var lectures = await _lectureRepository.GetByCourseAsync(courseId);

        if (lectures is null)
            throw new KeyNotFoundException($"Lectures with CourseId {courseId} not found");


        return _mapper.Map<IEnumerable<LectureDto>>(lectures);
    }

    public async Task<IEnumerable<LectureDto>> GetLecturesByTopicAsync(Guid topicId)
    {
        //TODO: add proper topic handler, not just an id
        var lectures = await _lectureRepository.GetByTopicAsync(topicId);

        if (lectures is null)
            throw new KeyNotFoundException($"Lectures with TopicId {topicId} not found");


        return _mapper.Map<IEnumerable<LectureDto>>(lectures);
    }

    public async Task<IEnumerable<LectureDto>> GetLectureByAuthorAsync(Guid authorId)
    {
        var lectures = await _lectureRepository.GetAsync(predicate: l => l.AuthorId.Equals(authorId));

        if (lectures is null)
            throw new KeyNotFoundException($"Lectures from Author {authorId} not found");

        return _mapper.Map<IEnumerable<LectureDto>>(lectures);
    }

    public async Task<LectureDto> CreateAsync(CreateLecture createLecture)
    {
        var validationResult = await _createValidator.ValidateAsync(createLecture);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _lectureRepository.ExistsAsync(l =>
                l.Title == createLecture.Title && l.CourseId.Equals(createLecture.CourseId) /*&& !l.IsDeleted*/))
            throw new InvalidOperationException($"Lecture with name {createLecture.Title} already exists in this course");

        var createLectureModel = _mapper.Map<LectureModel>(createLecture);
        createLectureModel.Id = Guid.NewGuid();
        createLectureModel.CreatedOn = createLectureModel.UpdatedOn = DateTime.UtcNow;

        var createdLecture = await _lectureRepository.AddAsync(createLectureModel);

        return _mapper.Map<LectureDto>(createdLecture);
    }

    public async Task<LectureDto> UpdateAsync(Guid lectureId, UpdateLecture updateLecture)
    {
        var validationResult = await _updateValidator.ValidateAsync(updateLecture);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingLecture = await GetEntityByIdAsync(lectureId);

        var updateLectureModel = _mapper.Map<LectureModel>(updateLecture);
        updateLectureModel.Id = existingLecture.Id;
        updateLectureModel.UpdatedOn = DateTime.UtcNow;

        var updatedLecture = await _lectureRepository.UpdateAsync(updateLectureModel);

        return _mapper.Map<LectureDto>(updatedLecture);
    }

    public async Task<LectureDto> DeleteAsync(Guid id)
    {
        var lecture = await GetEntityByIdAsync(id);

        await _lectureRepository.DeleteAsync(lecture);
        
        return _mapper.Map<LectureDto>(lecture);
    }

    public async Task<LectureDto> RestoreAsync(Guid id)
    {
        var lecture = await GetEntityByIdAsync(id);

        await _lectureRepository.DeleteAsync(lecture);
        
        return _mapper.Map<LectureDto>(lecture);
    }

    public async Task<LectureModel> GetEntityByIdAsync(Guid id)
    {
        var lecture = await _lectureRepository.GetByIdAsync(id);

        if (lecture is null)
            throw new KeyNotFoundException($"Lecture with ID {id} not found");

        return lecture;
    }
}
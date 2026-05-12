using AutoMapper;
using DPBloom.Application.Auth;
using DPBloom.Application.Exam.Contracts.Create;
using DPBloom.Application.Exam.Contracts.Update;
using DPBloom.Core.Exam;
using FluentValidation;

namespace DPBloom.Application.Exam;

public class ExamService : IExamService
{
    private readonly IExamRepository _examRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateExamDto> _createValidator;
    private readonly IValidator<UpdateExamDto> _updateValidator;
    private readonly ICurrentUserService _currentUserService;
    

    public ExamService(IExamRepository examRepository, IMapper mapper, IValidator<CreateExamDto> createValidator, IValidator<UpdateExamDto> updateValidator, ICurrentUserService currentUserService)
    {
        _examRepository = examRepository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUserService = currentUserService;
    }

    public async Task<ExamDetailsDto?> GetExamDetailsAsync(Guid examId)
    {
        var exam = await _examRepository.GetWithQuestionsAsync(examId);

        if (exam is null)
            throw new KeyNotFoundException($"Exam with ID {examId} not found");

        return _mapper.Map<ExamDetailsDto>(exam);
    }

    public async Task<Guid> CreateExamAsync(Guid courseId, CreateExamDto createExam)
    {
        var validationResult = await _createValidator.ValidateAsync(createExam);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _examRepository.ExistsAsync(e =>
                e.Title == createExam.Title && e.CourseId.Equals(courseId)))
            throw new InvalidOperationException($"Exam with name {createExam.Title} already exists");

        var createExamModel = _mapper.Map<ExamAggregateModel>(createExam);
        createExamModel.Exam.AuthorId = _currentUserService.GetUserId();
        createExamModel.Exam.CourseId = courseId;
        foreach (var question in createExamModel.Questions)
            question.Id = Guid.NewGuid();

        if (createExamModel.AnswerOptions is not null)
            foreach (var option in createExamModel.AnswerOptions)
                option.Id = Guid.NewGuid();
        
        var createdExam = await _examRepository.AddExamWithDetailsAsync(createExamModel);
        
        return createdExam.Exam.Id;
    }

    public async Task<ExamDetailsDto> UpdateExamAsync(Guid examId, UpdateExamDto updateExam)
    {
        var validationResult = await _updateValidator.ValidateAsync(updateExam);
        
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        var existingExam = await GetEntityByIdAsync(examId);
        //TODO: maybe incorrect mapping
        
        updateExam.CourseId ??= existingExam.Exam.CourseId;
        
        updateExam.TopicId ??= existingExam.Exam.TopicId;
        
        var updatedExistedExamModel = _mapper.Map(updateExam, existingExam);
        
        updatedExistedExamModel.Exam.UpdatedOn = DateTime.UtcNow;
        
        var updatedExam = await _examRepository.UpdateExamWithDetailsAsync(updatedExistedExamModel);
        
        return _mapper.Map<ExamDetailsDto>(updatedExam);
    }

    public async Task<ExamDetailsDto> DeleteExamAsync(Guid examId)
    {
        var exam = await GetEntityByIdAsync(examId);

        await _examRepository.DeleteExamWithDetailsAsync(exam);
        
        return _mapper.Map<ExamDetailsDto>(exam);
        
    }

    public async Task<ExamDetailsDto> RestoreExamAsync(Guid examId)
    {
        var exam = await GetEntityByIdAsync(examId);
        
        await _examRepository.RestoreExamWithDetailsAsync(exam);
        
        return _mapper.Map<ExamDetailsDto>(exam);
    }
    
    public async Task<ExamAggregateModel> GetEntityByIdAsync(Guid examId)
    {
        var lecture = await _examRepository.GetWithQuestionsAsync(examId);

        if (lecture is null)
            throw new KeyNotFoundException($"Lecture with ID {examId} not found");

        return lecture;
    }
}
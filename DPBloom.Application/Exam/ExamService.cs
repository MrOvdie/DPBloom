using AutoMapper;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Core.Exam;
using FluentValidation;
using TestOfTesting.DTOs;

namespace DPBloom.Application.Exam;

public class ExamService : IExamService
{
    private readonly IExamRepository _examRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateExamDto> _createValidator;
    private readonly IValidator<UpdateExamDto> _updateValidator;

    public ExamService(IExamRepository examRepository, IMapper mapper, IValidator<CreateExamDto> createValidator, IValidator<UpdateExamDto> updateValidator)
    {
        _examRepository = examRepository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ExamDetailsDto?> GetExamDetailsAsync(Guid id)
    {
        var exam = await _examRepository.GetWithQuestionsAsync(id);

        if (exam is null)
            throw new KeyNotFoundException($"Exam with ID {id} not found");

        return _mapper.Map<ExamDetailsDto>(exam);
    }

    public async Task<Guid> CreateExamAsync(CreateExamDto createExam)
    {
        var validationResult = await _createValidator.ValidateAsync(createExam);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _examRepository.ExistsAsync(e =>
                e.Title == createExam.Title && e.CourseId.Equals(createExam.CourseId)/* && !e.IsDeleted*/))
            throw new InvalidOperationException($"Exam with name {createExam.Title} already exists");

        var createExamModel = _mapper.Map<ExamAggregateModel>(createExam);
        createExamModel.Exam.Id = Guid.NewGuid();
        foreach (var question in createExamModel.Questions)
            question.Id = Guid.NewGuid();

        if (createExamModel.AnswerOptions != null)
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
        var updateExamModel = _mapper.Map<ExamAggregateModel>(updateExam);
        updateExamModel.Exam.Id = existingExam.Exam.Id;
        updateExamModel.Exam.UpdatedOn = DateTime.UtcNow;
        
        var updatedExam = await _examRepository.UpdateExamWithDetailsAsync(updateExamModel);
        
        return _mapper.Map<ExamDetailsDto>(updatedExam);
    }

    public async Task<ExamDetailsDto> DeleteExamAsync(Guid id)
    {
        var exam = await GetEntityByIdAsync(id);

        await _examRepository.DeleteExamWithDetailsAsync(exam);
        
        return _mapper.Map<ExamDetailsDto>(exam);
        
    }

    public async Task<ExamDetailsDto> RestoreExamAsync(Guid id)
    {
        var exam = await GetEntityByIdAsync(id);
        
        await _examRepository.RestoreExamWithDetailsAsync(exam);
        
        return _mapper.Map<ExamDetailsDto>(exam);
    }
    
    public async Task<ExamAggregateModel> GetEntityByIdAsync(Guid id)
    {
        var lecture = await _examRepository.GetWithQuestionsAsync(id);

        if (lecture is null)
            throw new KeyNotFoundException($"Lecture with ID {id} not found");

        return lecture;
    }
}
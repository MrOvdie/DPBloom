using AutoMapper;
using DPBloom.Application.Auth;
using DPBloom.Application.Enrollment;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Application.Exam.Contracts.Create;
using DPBloom.Application.Exam.Contracts.Update;
using DPBloom.Core.Exam;
using FluentValidation;
using UUIDNext;

namespace DPBloom.Application.Exam;

public class ExamService : IExamService
{
    private readonly IValidator<CreateExamDto> _createValidator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IExamRepository _examRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateExamDto> _updateValidator;


    public ExamService(IExamRepository examRepository, IMapper mapper, IValidator<CreateExamDto> createValidator,
        IValidator<UpdateExamDto> updateValidator, ICurrentUserService currentUserService,
        IEnrollmentRepository enrollmentRepository)
    {
        _examRepository = examRepository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _currentUserService = currentUserService;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<IReadOnlyList<ExamRecordDto>> GetAllExamsAsync()
    {
        var isAdmin = _currentUserService.IsAdmin();
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only admins can view all exams.");

        var exams = await _examRepository.GetAllAsync();

        return _mapper.Map<IReadOnlyList<ExamRecordDto>>(exams);
    }

    public async Task<ExamRecordDto> GetExamOverviewByIdAsync(Guid examId)
    {
        await EnsureHasAccessToGenericExamContent(examId);

        var exam = await _examRepository.GetByIdAsync(examId);
        if (exam is null)
            throw new KeyNotFoundException($"Exam with ID {examId} not found");

        return _mapper.Map<ExamRecordDto>(exam);
    }

    public async Task<ExamDetailsDto?> GetExamDetailsAsync(Guid examId)
    {
        await EnsureHasAccessToGenericExamContent(examId);

        var examAggregate = await _examRepository.GetWithQuestionsAsync(examId);
        if (examAggregate is null)
            throw new KeyNotFoundException($"Exam with ID {examId} not found");
        
        var dto = _mapper.Map<ExamDetailsDto>(examAggregate.Exam);

        dto.Questions = _mapper.Map<List<QuestionDto>>(examAggregate.Questions);
        
        foreach (var questionDto in dto.Questions)
        {
            if (examAggregate.AnswerOptions is not null)
            {
                var optionsForQuestion = examAggregate.AnswerOptions
                    .Where(o => o.QuestionId == questionDto.Id)
                    .ToList();
            
                questionDto.Options = _mapper.Map<List<OptionDto>>(optionsForQuestion);
            }
        }
        
        return dto;
    }

    public async Task<IReadOnlyList<ExamRecordDto>> GetExamsByCourseAsync(Guid courseId)
    {
        await EnsureHasAccessToGenericExamContent(courseId);

        var exams = await _examRepository.GetByCourseAsync(courseId);

        return _mapper.Map<IReadOnlyList<ExamRecordDto>>(exams);
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
        createExamModel.Exam.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);
        createExamModel.Exam.CreatedOn = createExamModel.Exam.UpdatedOn = DateTime.UtcNow;
        createExamModel.Exam.AuthorId = createExamModel.Exam.LastUpdaterId = _currentUserService.GetUserId();
        createExamModel.Exam.CourseId = courseId;
        createExamModel.Exam.MaximumScore = createExam.Questions.Select(eq => eq.ScoreWeight).Sum();

        var questionsDtoList = createExam.Questions.ToList();
        var questionsModelList = createExamModel.Questions.ToList();

        for (var i = 0; i < createExamModel.Questions.Count; i++)
        {
            var questionDto = questionsDtoList[i];
            var questionModel = questionsModelList[i];

            questionModel.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);
            questionModel.ExamId = createExamModel.Exam.Id;
            questionModel.CreatedOn = questionModel.UpdatedOn = DateTime.UtcNow;

            if (questionDto.Options is not null)
            {
                var optionsModels = _mapper.Map<List<AnswerOptionModel>>(questionDto.Options);

                foreach (var optionModel in optionsModels)
                {
                    optionModel.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);
                    optionModel.QuestionId = questionModel.Id; // Тепер ми точно знаємо цей ID!
                    optionModel.CreatedOn = optionModel.UpdatedOn = DateTime.UtcNow;

                    createExamModel.AnswerOptions.Add(optionModel);
                }
            }
        }

        var createdExam = await _examRepository.AddExamWithDetailsAsync(createExamModel);

        return createdExam.Exam.Id;
    }

    public async Task<ExamDetailsDto> UpdateExamAsync(Guid examId, UpdateExamDto updateDto)
    {
        var validationResult = await _updateValidator.ValidateAsync(updateDto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        var existingExam = await _examRepository.GetWithQuestionsAsync(examId);
        if (existingExam is null)
            throw new KeyNotFoundException("Exam not found");

        updateDto.Id = examId;
        
        _mapper.Map(updateDto, existingExam.Exam);
        existingExam.Exam.UpdatedOn = DateTime.UtcNow;
        existingExam.Exam.LastUpdaterId = _currentUserService.GetUserId();

        if (updateDto.Questions is null)
        {
            await _examRepository.UpdateExamWithDetailsAsync(existingExam);
            return _mapper.Map<ExamDetailsDto>(existingExam);
        }

        var incomingQuestionIds = updateDto.Questions
            .Where(q => q.Id.HasValue && q.Id.Value != Guid.Empty)
            .Select(q => q.Id.Value)
            .ToList();

        var incomingOptionIds = updateDto.Questions
            .Where(q => q.Options is not null)
            .SelectMany(q => q.Options!)
            .Where(o => o.Id.HasValue && o.Id.Value != Guid.Empty)
            .Select(o => o.Id.Value)
            .ToList();

        var questionsToRemove = existingExam.Questions.Where(q => !incomingQuestionIds.Contains(q.Id)).ToList();
        foreach (var q in questionsToRemove) existingExam.Questions.Remove(q);

        var optionsToRemove = existingExam.AnswerOptions.Where(o => !incomingOptionIds.Contains(o.Id)).ToList();
        foreach (var o in optionsToRemove) existingExam.AnswerOptions.Remove(o);


        foreach (var qDto in updateDto.Questions)
        {
            Guid currentQuestionId;

            if (qDto.Id.HasValue && qDto.Id.Value != Guid.Empty)
            {
                var existingQuestion = existingExam.Questions.FirstOrDefault(q => q.Id == qDto.Id.Value);
                if (existingQuestion is not null)
                {
                    _mapper.Map(qDto, existingQuestion);
                    existingQuestion.UpdatedOn = DateTime.UtcNow;
                    currentQuestionId = existingQuestion.Id;
                }
                else continue;
            }
            else
            {
                var newQuestion = _mapper.Map<QuestionModel>(qDto);
                newQuestion.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);
                newQuestion.ExamId = existingExam.Exam.Id;
                newQuestion.CreatedOn = newQuestion.UpdatedOn = DateTime.UtcNow;

                existingExam.Questions.Add(newQuestion);
                currentQuestionId = newQuestion.Id;
            }

            if (qDto.Options is not null)
            {
                foreach (var optDto in qDto.Options)
                {
                    if (optDto.Id.HasValue && optDto.Id.Value != Guid.Empty)
                    {
                        var existingOpt = existingExam.AnswerOptions.FirstOrDefault(o => o.Id == optDto.Id.Value);
                        if (existingOpt is not null)
                        {
                            _mapper.Map(optDto, existingOpt);
                            existingOpt.UpdatedOn = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        var newOpt = _mapper.Map<AnswerOptionModel>(optDto);
                        newOpt.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);

                        newOpt.QuestionId = currentQuestionId;
                        newOpt.CreatedOn = newOpt.UpdatedOn = DateTime.UtcNow;

                        existingExam.AnswerOptions.Add(newOpt);
                    }
                }
            }
        }

        existingExam.Exam.MaximumScore = existingExam.Questions.Sum(q => q.ScoreWeight);
        var updatedExamAggregateModel = await _examRepository.UpdateExamWithDetailsAsync(existingExam);

        return updatedExamAggregateModel.ToDetailsDto(_mapper);
    }
    /*public async Task<ExamDetailsDto> UpdateExamAsync(Guid examId, UpdateExamDto updateExam)
    {
        var validationResult = await _updateValidator.ValidateAsync(updateExam);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        await EnsureUserHasAccessToExamModifyingAsync(examId);

        var existingExam = await GetEntityByIdAsync(examId);

        updateExam.CourseId ??= existingExam.Exam.CourseId;

        updateExam.TopicId ??= existingExam.Exam.TopicId;

        var updatedExistedExamModel = _mapper.Map(updateExam, existingExam);
        updatedExistedExamModel.Exam.UpdatedOn = DateTime.UtcNow;
        updatedExistedExamModel.Exam.LastUpdaterId = _currentUserService.GetUserId();

        var updatedExam = await _examRepository.UpdateExamWithDetailsAsync(updatedExistedExamModel);

        return _mapper.Map<ExamDetailsDto>(updatedExam);
    }*/
    
    public async Task<ExamDetailsDto> DeleteExamAsync(Guid examId)
    {
        await EnsureUserHasAccessToExamModifyingAsync(examId);

        var exam = await GetEntityByIdAsync(examId);

        await _examRepository.DeleteExamWithDetailsAsync(exam);

        return _mapper.Map<ExamDetailsDto>(exam);
    }

    public async Task<ExamDetailsDto> RestoreExamAsync(Guid examId)
    {
        await EnsureUserHasAccessToExamModifyingAsync(examId);

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

    private async Task EnsureStudentIsEnrolledAsync(Guid examId, Guid userId)
    {
        var courseId = await _examRepository.GetCourseIdByExamIdAsync(examId);
        if (courseId is null)
            throw new KeyNotFoundException("Course not found.");

        var isEnrolled = await _enrollmentRepository.ExistsAsync(userId, courseId.Value);
        if (!isEnrolled)
            throw new KeyNotFoundException("User is not enrolled in this course.");
    }

    private async Task EnsureHasAccessToGenericExamContent(Guid examId)
    {
        if (_currentUserService.IsAdmin()) return;

        var currentUserId = _currentUserService.GetUserId();

        var teacherId = await _examRepository.IsExamAuthorAsync(examId, currentUserId);
        if (teacherId) return;

        await EnsureStudentIsEnrolledAsync(examId, currentUserId);
    }

    private async Task EnsureUserHasAccessToExamModifyingAsync(Guid examId)
    {
        var isAdmin = _currentUserService.IsAdmin();
        if (isAdmin)
            return;

        var userId = _currentUserService.GetUserId();
        var isAuthor = await _examRepository.IsExamAuthorAsync(examId, userId);

        if (!isAuthor)
        {
            var examExists = await _examRepository.ExistsAsync(l => l.Id.Equals(examId));
            if (!examExists)
                throw new KeyNotFoundException("Course not found.");

            throw new UnauthorizedAccessException("Only author or admins can modify this exam.");
        }
    }
}
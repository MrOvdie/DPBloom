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
        IValidator<UpdateExamDto> updateValidator, ICurrentUserService currentUserService, IEnrollmentRepository enrollmentRepository)
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

    public async Task<ExamDetailsDto?> GetExamDetailsAsync(Guid examId)
    {
        await EnsureHasAccessToGenericExamContent(examId);
        
        var exam = await _examRepository.GetWithQuestionsAsync(examId);

        if (exam is null)
            throw new KeyNotFoundException($"Exam with ID {examId} not found");

        return _mapper.Map<ExamDetailsDto>(exam);
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
        createExamModel.Exam.AuthorId = _currentUserService.GetUserId();
        createExamModel.Exam.CourseId = courseId;
        createExamModel.Exam.MaximumScore = createExam.Questions.Select(eq => eq.ScoreWeight).Sum();

        foreach (var question in createExamModel.Questions)
            question.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);

        if (createExamModel.AnswerOptions is not null)
            foreach (var option in createExamModel.AnswerOptions)
                option.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);

        var createdExam = await _examRepository.AddExamWithDetailsAsync(createExamModel);

        return createdExam.Exam.Id;
    }

    public async Task<ExamDetailsDto> UpdateExamAsync(Guid examId, UpdateExamDto updateExam)
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
    }

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
using AutoMapper;
using DPBloom.Application.Auth;
using DPBloom.Application.Enrollment;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Core.Exam;
using DPBloom.Core.Exam.Enums;
using FluentValidation;
using UUIDNext;
using Type = DPBloom.Core.Exam.Enums.Type;

namespace DPBloom.Application.Exam;

public class AttemptService : IAttemptService
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly IAttemptResultRepository _attemptResultRepository;
    private readonly IExamRepository _examRepository;
    private readonly IExamService _examService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IValidator<SubmitAnswerDto> _submitAnswerValidator;
    private readonly IValidator<TeacherEvaluationDto> _teacherEvaluationValidator;
    private readonly IMapper _mapper;

    public AttemptService(IAttemptRepository attemptRepository, IMapper mapper, IExamRepository examRepository,
        IExamService examService, IAttemptResultRepository attemptResultRepository,
        ICurrentUserService currentUserService, IEnrollmentRepository enrollmentRepository,
        IValidator<SubmitAnswerDto> submitAnswerValidator, IValidator<TeacherEvaluationDto> teacherEvaluationValidator)
    {
        _attemptResultRepository = attemptResultRepository;
        _currentUserService = currentUserService;
        _enrollmentRepository = enrollmentRepository;
        _submitAnswerValidator = submitAnswerValidator;
        _teacherEvaluationValidator = teacherEvaluationValidator;
        _attemptRepository = attemptRepository;
        _examRepository = examRepository;
        _examService = examService;
        _mapper = mapper;
    }

    public async Task<Guid> StartAsync(Guid examId)
    {
        var exam = await _examService.GetEntityByIdAsync(examId);

        if (exam.Exam.StartsAt > DateTime.UtcNow || exam.Exam.FinishesAt < DateTime.UtcNow)
            throw new InvalidOperationException("Exam is not available for start");

        var userId = _currentUserService.GetUserId();
        
        await EnrollmentCheck(exam.Exam.CourseId, userId); //Check this

        var userAttempt = new UserExamAttemptModel
        {
            Id = Uuid.NewDatabaseFriendly(Database.SqlServer),
            UserId = userId,
            ExamId = examId,
            CourseId = exam.Exam.CourseId,
            Status = AttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };
        var startedModel = await _attemptRepository.StartAsync(userAttempt);

        return startedModel.Id;
    }

    public async Task SubmitAnswerAsync(Guid attemptId, SubmitAnswerDto answer)
    {
        var userId = _currentUserService.GetUserId();
        
        await ValidateAttemptAccessAndStatusAsync(userId, attemptId);

        var validationResult = await _submitAnswerValidator.ValidateAsync(answer);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var answerModel = _mapper.Map<UserQuestionAnswerModel>(answer);
        answerModel.AttemptId = attemptId;

        await _attemptRepository.SubmitAnswerAsync(attemptId, answerModel);
    }

    public async Task SaveAllAnswersAsync(Guid attemptId, List<SubmitAnswerDto> answers)
    {
        var userId = _currentUserService.GetUserId();
        
        await ValidateAttemptAccessAndStatusAsync(userId, attemptId);

        var validationResults = await Task.WhenAll(answers.Select(a => _submitAnswerValidator.ValidateAsync(a)));

        if (!validationResults.All(r => r.IsValid))
            throw new ValidationException(validationResults.SelectMany(r => r.Errors));

        var answerModels = _mapper.Map<List<UserQuestionAnswerModel>>(answers);

        foreach (var answerModel in answerModels)
        {
            answerModel.AttemptId = attemptId;
        }

        await _attemptRepository.SaveAllAnswersAsync(attemptId, answerModels);
    }

    public async Task<AttemptResultDto> FinishAsync(Guid attemptId)
    {
        var userId = _currentUserService.GetUserId();
        
        var attempt = await ValidateAttemptAccessAndStatusAsync(userId, attemptId);

        if (attempt.Status is AttemptStatus.Submitted or AttemptStatus.Expired)
        {
            return await GetResultAsync(attemptId);
        }

        if (attempt.Status is not AttemptStatus.InProgress)
        {
            throw new InvalidOperationException("This attempt cannot be finished.");
        }

        var exam = await _examRepository.GetWithQuestionsAsync(attempt.ExamId);

        attempt.FinishedAt = DateTime.UtcNow;

        var isTimeOver = attempt.FinishedAt > exam.Exam.FinishesAt.AddSeconds(10) ||
                         attempt.FinishedAt - attempt.StartedAt > exam.Exam.Duration.Add(TimeSpan.FromSeconds(10));

        attempt.Status = isTimeOver ? AttemptStatus.Expired : AttemptStatus.Submitted;

        await _attemptRepository.FinishAsync(attempt);

        var resultModel = await CalculateAndSaveResultAsync(attempt, exam);
        return _mapper.Map<AttemptResultDto>(resultModel);
    }

    public async Task<AttemptResultDto> GetResultAsync(Guid attemptId)
    {
        var attempt = await GetEntityByIdAsync(attemptId);

        if (attempt is null)
            throw new KeyNotFoundException($"Can't find attempt with id {attemptId}.");

        if (attempt.Status is not AttemptStatus.Submitted and not AttemptStatus.Expired)
            throw new InvalidOperationException("This attempt isn't finished.");

        var attemptResultId = attempt.AttemptResultId;

        if (attemptResultId is null)
        {
            var exam = await _examRepository.GetWithQuestionsAsync(attempt.ExamId);
            var resultModel = await CalculateAndSaveResultAsync(attempt, exam);

            return _mapper.Map<AttemptResultDto>(resultModel);
        }

        await EnsureUserHasAccessToAttemptResultAsync(attemptResultId.Value);

        var savedResult = await _attemptResultRepository.GetByIdAsync(attemptResultId.Value);

        return _mapper.Map<AttemptResultDto>(savedResult);
    }
    
    public async Task<IReadOnlyList<AttemptResultRecordDto>> GetAttemptResultsByExamAsync(Guid examId)
    {
        var exam = await _examRepository.GetByIdAsync(examId);
        if (exam is null)
            throw new KeyNotFoundException($"Can't find exam with id {examId}.");
        
        var userId = _currentUserService.GetUserId();
        var isAdmin = _currentUserService.IsAdmin();
        
        if (exam.AuthorId != userId && !isAdmin)
            throw new UnauthorizedAccessException("You are not allowed to access this exam attempt results.");
        
        var attemptResults = await _attemptResultRepository
            .GetAllAttemptsResultsByExamAsync(examId);

        return _mapper.Map<IReadOnlyList<AttemptResultRecordDto>>(attemptResults);
    }


    private async Task<UserExamAttemptModel> GetEntityByIdAsync(Guid id)
    {
        var examAttempt = await _attemptRepository.GetByIdAsync(id);

        if (examAttempt is null)
            throw new KeyNotFoundException($"Exam attempt with ID {id} not found");

        return examAttempt;
    }
    
    private async Task<AttemptResultModel> CalculateAndSaveResultAsync(UserExamAttemptModel attempt,
        ExamAggregateModel exam)
    {
        if (attempt.AttemptResultId.HasValue)
        {
            return await _attemptResultRepository.GetByIdAsync(attempt.AttemptResultId.Value);
        }

        var result = new AttemptResultModel
        {
            Id = Uuid.NewDatabaseFriendly(Database.SqlServer),
            AttemptId = attempt.Id,
            ExamId = exam.Exam.Id,
            CourseId = exam.Exam.CourseId,
            TotalQuestions = exam.Questions.Count,
            Details = [],
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };

        var allManualReviews = await _attemptResultRepository.GetAllManualReviewsForAttemptAsync(attempt.Id);

        foreach (var question in exam.Questions)
        {
            QuestionResultModel questionReview;

            if (question.CheckingType == CheckingType.Automatic)
            {
                questionReview = await CheckCorrectAnswersForQuestionAsync(attempt.Id, question.Id, exam);
            }
            else
            {
                var manualReview = allManualReviews.FirstOrDefault(r => r.QuestionId == question.Id);

                if (manualReview is null)
                {
                    questionReview = new QuestionResultModel
                    {
                        Id = Uuid.NewDatabaseFriendly(Database.SqlServer),
                        QuestionId = question.Id,
                        Text = question.Text,
                        SelectedOptionIds = new List<Guid>(),
                        CorrectOptionIds = new List<Guid>(),
                        Score = 0,
                        IsCorrect = false,
                        QuestionResultStatus = AttemptStatus.PendingManualReview
                    };
                }
                else
                {
                    questionReview = _mapper.Map<QuestionResultModel>(manualReview);
                }
            }

            result.Details.Add(questionReview);
        }

        var (updatedResult, hasPending) = UpdateAttemptResultMetrics(result, exam);

        await _attemptResultRepository.SaveAttemptResultAsync(attempt.Id, updatedResult);

        attempt.AttemptResultId = updatedResult.Id;
        attempt.Status = hasPending ? AttemptStatus.PendingManualReview : AttemptStatus.Checked;

        await _attemptRepository.UpdateAsync(attempt);

        return updatedResult;
    }

    private async Task<QuestionResultModel> CheckCorrectAnswersForQuestionAsync(Guid attemptId, Guid questionId,
        ExamAggregateModel exam)
    {
        var question = exam.Questions.SingleOrDefault(q => q.Id.Equals(questionId));
        if (question is null)
            throw new InvalidOperationException($"Question {questionId} not found in exam.");

        var userAnswers = await _examRepository.GetUserAnswersForQuestionAsync(attemptId, questionId);
        var selectedOptionIds = userAnswers.SelectMany(ua => ua.SelectedOptionIds).ToList();

        var correctOptions = exam.AnswerOptions
            .Where(ao => ao.QuestionId.Equals(questionId) && ao.IsCorrect)
            .ToList();

        var correctOptionIds = correctOptions.Select(ao => ao.Id).ToList();

        var result = new QuestionResultModel
        {
            Id = Uuid.NewDatabaseFriendly(Database.SqlServer),
            QuestionId = questionId,
            Text = question.Text,
            SelectedOptionIds = selectedOptionIds,
            CorrectOptionIds = correctOptionIds,
            Score = 0,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };

        switch (question.Type)
        {
            case Type.SingleChoice:
                result.IsCorrect = selectedOptionIds.Count == 1 &&
                                   selectedOptionIds.First() == correctOptionIds.First();
                result.Score = result.IsCorrect ? question.ScoreWeight : 0;
                break;

            case Type.MultipleChoice:
                var correctCount = correctOptionIds.Count;
                var weightPerCorrect = question.ScoreWeight / correctCount;

                var correctSelected = selectedOptionIds.Intersect(correctOptionIds).Count();
                var incorrectSelected = selectedOptionIds.Except(correctOptionIds).Count();

                var score = Math.Max(0, (correctSelected - incorrectSelected) * weightPerCorrect);

                var isFullyCorrect = correctSelected == correctCount && incorrectSelected == 0;

                if (isFullyCorrect)
                {
                    score = question.ScoreWeight;
                }

                result.Score = score;
                result.IsCorrect = isFullyCorrect;
                break;

            case Type.OpenAnswer:
                var userText = userAnswers.FirstOrDefault()?.FreeTextAnswer?.Trim();

                var isMatch = correctOptions.Any(ao => 
                    string.Equals(ao.Text.Trim(), userText, StringComparison.OrdinalIgnoreCase));

                result.IsCorrect = isMatch;
                result.Score = result.IsCorrect ? question.ScoreWeight : 0;
                break;
        }

        return result;
    }

    public async Task<AttemptResultDto> CheckOpenTextAnswerAsync(Guid attemptResultId,
        IReadOnlyList<TeacherEvaluationDto> teacherEvaluations,
        Guid examId)
    {
        var validationResults =
            await Task.WhenAll(teacherEvaluations
                .Select(a => _teacherEvaluationValidator.ValidateAsync(a)));

        if (!validationResults.All(r => r.IsValid))
            throw new ValidationException(validationResults.SelectMany(r => r.Errors));

        var attemptResult = await _attemptResultRepository.GetByIdAsync(attemptResultId);
        if (attemptResult is null)
            throw new KeyNotFoundException($"Can't find attempt result with id {attemptResultId}.");

        var exam = await _examRepository.GetWithQuestionsAsync(examId);
        if (exam is null)
            throw new KeyNotFoundException($"Can't find exam with id {examId}.");
        
        var user = _currentUserService.GetUserId();
        
        if (exam.Exam.AuthorId != user)
            throw new UnauthorizedAccessException("You are not allowed to perform this action.");

        foreach (var evaluation in teacherEvaluations)
        {
            var questionResult = attemptResult.Details.FirstOrDefault(q => q.QuestionId == evaluation.QuestionId);
            if (questionResult is null)
                throw new KeyNotFoundException(
                    $"Can't find answer for question {evaluation.QuestionId} in this attempt result {attemptResultId}.");

            var questionDefinition = exam.Questions.First(q => q.Id == evaluation.QuestionId);

            var finalScore = Math.Min(evaluation.AwardedScore, questionDefinition.ScoreWeight);

            questionResult.Score = finalScore;
            questionResult.IsCorrect = finalScore > 0;
            questionResult.QuestionResultStatus = AttemptStatus.Checked;
        }

        var (updatedResult, hasPending) = UpdateAttemptResultMetrics(attemptResult, exam);

        await _attemptResultRepository.UpdateAsync(updatedResult);

        if (!hasPending)
        {
            var attempt = await _attemptRepository.GetByIdAsync(updatedResult.AttemptId);
            if (attempt is not null)
            {
                attempt.Status = AttemptStatus.Checked;
                await _attemptRepository.UpdateAsync(attempt);
            }
        }

        return _mapper.Map<AttemptResultDto>(updatedResult);
    }

    public async Task<IReadOnlyList<AttemptResultRecordDto>> GetAttemptResultsForManualReviewByExamAsync(Guid examId)
    {
        var exam = await _examRepository.GetByIdAsync(examId);
        if (exam is null)
            throw new KeyNotFoundException($"Can't find exam with id {examId}.");

        var user = _currentUserService.GetUserId();
        var isAdmin = _currentUserService.IsAdmin();
        if (exam.AuthorId != user && !isAdmin)
            throw new UnauthorizedAccessException("You are not allowed to access this exam.");

        var manualAttemptResults = await _attemptResultRepository.GetAllManualReviewsAttemptsForExamAsync(exam.Id);

        return _mapper.Map<List<AttemptResultRecordDto>>(manualAttemptResults);
    }


    private async Task EnsureUserHasAccessToAttemptResultAsync(Guid attemptResultId)
    {
        if (_currentUserService.IsAdmin())
            return;

        var currentUserId = _currentUserService.GetUserId();

        var accessInfo = await _attemptResultRepository.GetAccessInfoAsync(attemptResultId);

        if (accessInfo is null)
            throw new KeyNotFoundException("Attempt result not found.");

        if (accessInfo.StudentId != currentUserId && accessInfo.TeacherId != currentUserId)
            throw new UnauthorizedAccessException("Only the student, the course author, or admins can view this result.");
    }

    private async Task EnrollmentCheck(Guid courseId, Guid userId)
    {
        var enrollmentCheck = await _enrollmentRepository.ExistsAsync(userId, courseId);

        if (!enrollmentCheck)
            throw new UnauthorizedAccessException("You are not enrolled in this course.");
    }

    private async Task<UserExamAttemptModel> ValidateAttemptAccessAndStatusAsync(Guid userId, Guid attemptId)
    {
        var attempt = await _attemptRepository.GetByIdAsync(attemptId);

        if (attempt is null)
            throw new KeyNotFoundException($"Can't find attempt with id {attemptId}.");

        if (attempt.UserId != userId)
            throw new UnauthorizedAccessException("You are not allowed to access this attempt.");

        await EnrollmentCheck(attempt.CourseId, userId);

        if (attempt.Status == AttemptStatus.InProgress)
        {
            var exam = await _examRepository.GetWithQuestionsAsync(attempt.ExamId);

            if (exam is null)
                throw new KeyNotFoundException("Exam not found.");

            var now = DateTime.UtcNow;

            var gracePeriod = TimeSpan.FromSeconds(10);

            var isTimeOver = (now > exam.Exam.FinishesAt.Add(gracePeriod)) ||
                             (now > attempt.StartedAt.Add(exam.Exam.Duration).Add(gracePeriod));

            if (isTimeOver)
            {
                attempt.Status = AttemptStatus.Expired;
                attempt.FinishedAt = now;
                await _attemptRepository.UpdateAsync(attempt);

                await CalculateAndSaveResultAsync(attempt, exam);

                throw new InvalidOperationException("Time is over. Your attempt has been saved.");
            }
        }

        if (attempt.Status is not AttemptStatus.InProgress)
            throw new InvalidOperationException("This attempt has already been finished or expired.");

        return attempt;
    }

    private (AttemptResultModel Result, bool HasPending) UpdateAttemptResultMetrics(
        AttemptResultModel attemptResult,
        ExamAggregateModel exam)
    {
        attemptResult.Score = attemptResult.Details.Sum(d => d.Score);
        attemptResult.CorrectAnswers = attemptResult.Details.Count(d => d.IsCorrect);

        var totalPossibleScore = exam.Questions.Sum(q => q.ScoreWeight);
        attemptResult.ScorePercentage = totalPossibleScore > 0
            ? Math.Round((attemptResult.Score / totalPossibleScore) * 100, 2)
            : 0;

        var hasPending = attemptResult.Details
            .Any(d => exam.Questions.First(q => q.Id == d.QuestionId).CheckingType == CheckingType.Manual
                      && d.QuestionResultStatus != AttemptStatus.Checked);

        attemptResult.Passed = !hasPending &&
                               (exam.Exam.MinimalPassScore is null ||
                                attemptResult.Score >= exam.Exam.MinimalPassScore.Value);

        attemptResult.UpdatedOn = DateTime.UtcNow;

        return (attemptResult, hasPending);
    }
}
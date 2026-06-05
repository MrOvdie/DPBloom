using AutoMapper;
using DPBloom.Application.Attempt.Contracts;
using DPBloom.Application.Auth;
using DPBloom.Application.Bloom;
using DPBloom.Application.Enrollment;
using DPBloom.Application.Exam;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Application.User;
using DPBloom.Core.Exam;
using DPBloom.Core.Exam.Enums;
using FluentValidation;
using UUIDNext;

namespace DPBloom.Application.Attempt;

public class AttemptService : IAttemptService
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly IAttemptResultRepository _attemptResultRepository;
    private readonly IExamRepository _examRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IValidator<SubmitAnswerDto> _submitAnswerValidator;
    private readonly IValidator<TeacherEvaluationDto> _teacherEvaluationValidator;
    private readonly IBloomService _bloomService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public AttemptService(IAttemptRepository attemptRepository, IMapper mapper, IExamRepository examRepository,
        IAttemptResultRepository attemptResultRepository,
        ICurrentUserService currentUserService, IEnrollmentRepository enrollmentRepository,
        IValidator<SubmitAnswerDto> submitAnswerValidator, IValidator<TeacherEvaluationDto> teacherEvaluationValidator,
        IBloomService bloomService, IUserService userService)
    {
        _attemptResultRepository = attemptResultRepository;
        _currentUserService = currentUserService;
        _enrollmentRepository = enrollmentRepository;
        _submitAnswerValidator = submitAnswerValidator;
        _teacherEvaluationValidator = teacherEvaluationValidator;
        _bloomService = bloomService;
        _userService = userService;
        _attemptRepository = attemptRepository;
        _examRepository = examRepository;
        _mapper = mapper;
    }

    public async Task<Guid> StartAsync(Guid examId)
    {
        var exam = await _examRepository.GetWithQuestionsAsync(examId);

        if (exam.Exam.StartsAt > DateTime.UtcNow || exam.Exam.FinishesAt < DateTime.UtcNow)
            throw new InvalidOperationException("Exam is not available for start");

        var userId = _currentUserService.GetUserId();

        await EnrollmentCheck(exam.Exam.CourseId, userId);

        var activeAttemptId = await _attemptRepository.GetActiveAttemptIdAsync(userId, examId);

        if (activeAttemptId.HasValue)
        {
            return activeAttemptId.Value;
        }

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

        var existingAnswer = await _attemptRepository.GetAnswerAsync(attemptId, answer.QuestionId);

        if (existingAnswer is not null)
        {
            existingAnswer.SelectedOptionIds = answer.SelectedOptionIds;
            existingAnswer.FreeTextAnswer = answer.FreeTextAnswer;
            existingAnswer.UpdatedOn = DateTime.UtcNow;
            existingAnswer.SubmittedAt = DateTime.UtcNow;

            await _attemptRepository.UpdateAnswerAsync(existingAnswer);
        }
        else
        {
            var newAnswer = _mapper.Map<UserQuestionAnswerModel>(answer);
            newAnswer.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);
            newAnswer.AttemptId = attemptId;
            newAnswer.CreatedOn = DateTime.UtcNow;
            newAnswer.UpdatedOn = DateTime.UtcNow;
            newAnswer.SubmittedAt = DateTime.UtcNow;

            await _attemptRepository.AddAnswerAsync(newAnswer);
        }
    }

    public async Task SaveAllAnswersAsync(Guid attemptId, List<SubmitAnswerDto> answers)
    {
        var userId = _currentUserService.GetUserId();

        await ValidateAttemptAccessAndStatusAsync(userId, attemptId);

        var validationResults = await Task.WhenAll(answers.Select(a => _submitAnswerValidator.ValidateAsync(a)));
        if (!validationResults.All(r => r.IsValid))
            throw new ValidationException(validationResults.SelectMany(r => r.Errors));

        var existingAnswers = await _attemptRepository.GetAnswersByAttemptIdAsync(attemptId);

        var answersToAdd = new List<UserQuestionAnswerModel>();
        var answersToUpdate = new List<UserQuestionAnswerModel>();

        foreach (var answerDto in answers)
        {
            var existingAnswer = existingAnswers.FirstOrDefault(a => a.QuestionId == answerDto.QuestionId);

            if (existingAnswer is not null)
            {
                existingAnswer.SelectedOptionIds = answerDto.SelectedOptionIds;
                existingAnswer.FreeTextAnswer = answerDto.FreeTextAnswer;
                existingAnswer.UpdatedOn = DateTime.UtcNow;
                existingAnswer.SubmittedAt = DateTime.UtcNow;

                answersToUpdate.Add(existingAnswer);
            }
            else
            {
                var newAnswer = _mapper.Map<UserQuestionAnswerModel>(answerDto);
                newAnswer.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);
                newAnswer.AttemptId = attemptId;
                newAnswer.CreatedOn = DateTime.UtcNow;
                newAnswer.UpdatedOn = DateTime.UtcNow;
                newAnswer.SubmittedAt = DateTime.UtcNow;

                answersToAdd.Add(newAnswer);
            }
        }

        await _attemptRepository.SaveAnswersBatchAsync(answersToAdd, answersToUpdate);
    }

    public async Task<AttemptDetailsDto> ContinueAttemptAsync(Guid attemptId)
    {
        var attempt = await _attemptRepository.GetByIdAsync(attemptId);
        if (attempt is null)
            throw new KeyNotFoundException($"Can't find attempt with Id {attemptId}.");
        if (attempt.Status is not AttemptStatus.InProgress)
            throw new InvalidOperationException("This attempt cannot be continued.");

        var exam = await _examRepository.GetWithQuestionsAsync(attempt.ExamId);

        var savedAnswers = await _attemptRepository.GetAnswersByAttemptIdAsync(attemptId);

        var allAttempts = await _attemptRepository.GetAttemptsByUserByExamIdAsync(attempt.UserId, attempt.ExamId);
        var attemptCount = allAttempts.Count(a =>
            a.UserId == attempt.UserId && a.ExamId == attempt.ExamId && a.StartedAt <= attempt.StartedAt);

        var attemptDto = new AttemptDetailsDto
        {
            Id = attempt.Id,
            ExamId = attempt.ExamId,
            ExamTitle = exam.Exam.Title,
            ExamDescription = exam.Exam.Description,
            CanSkip = exam.Exam.CanSkip,
            ShowResults = exam.Exam.ShowResults,
            IsRandomOrder = exam.Exam.IsRandomOrder,
            StartedAt = attempt.StartedAt,
            Duration = exam.Exam.Duration,
            AttemptNumber = attemptCount,

            Questions = _mapper.Map<List<QuestionDto>>(exam.Questions),

            SavedAnswers = _mapper.Map<List<SavedAnswerDto>>(savedAnswers)
        };

        foreach (var questionDto in attemptDto.Questions)
        {
            var optionsForThisQuestion = exam.AnswerOptions.Where(o => o.QuestionId == questionDto.Id);

            questionDto.Options = _mapper.Map<List<OptionDto>>(optionsForThisQuestion);
        }

        return attemptDto;
    }

    public async Task<AttemptResultDto> FinishAsync(Guid attemptId)
    {
        var userId = _currentUserService.GetUserId();

        var attempt = await ValidateAttemptAccessAndStatusAsync(userId, attemptId);

        if (attempt.Status is AttemptStatus.Submitted or AttemptStatus.Expired)
            return await GetResultAsync(attemptId);

        if (attempt.Status is not AttemptStatus.InProgress)
            throw new InvalidOperationException("This attempt cannot be finished.");

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
        if (attempt.Status is AttemptStatus.InProgress)
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

    public async Task<AttemptOverviewAggregateDto> GetResultOverviewAsync(Guid attemptId)
    {
        var attemptAggregate = await _attemptRepository.GetAttemptAggregateAsync(attemptId);
        if (attemptAggregate?.Attempt is null)
            throw new KeyNotFoundException($"Can't find attempt with id {attemptId}.");
        if (attemptAggregate.Attempt.Status is AttemptStatus.InProgress)
            throw new InvalidOperationException("This attempt isn't finished.");

        var exam = await _examRepository.GetWithQuestionsAsync(attemptAggregate.Attempt.ExamId);
        if (exam?.Exam is null)
            throw new KeyNotFoundException($"Can't find exam with id {attemptAggregate.Attempt.ExamId}.");

        AttemptResultModel resultModel;

        if (attemptAggregate.Attempt.AttemptResultId is null)
        {
            resultModel = await CalculateAndSaveResultAsync(attemptAggregate.Attempt, exam);
        }
        else
        {
            await EnsureUserHasAccessToAttemptResultAsync(attemptAggregate.Attempt.AttemptResultId.Value);

            resultModel =
                await _attemptResultRepository.GetByIdWithDetailsAsync(attemptAggregate.Attempt.AttemptResultId.Value);
            if (resultModel is null)
                throw new KeyNotFoundException(
                    $"Can't find attempt result with id {attemptAggregate.Attempt.AttemptResultId.Value}.");
        }

        var tempDetails = _mapper.Map<AttemptDetailsDto>(attemptAggregate);
        tempDetails.Duration = resultModel.Duration;
        tempDetails.AttemptNumber = await _attemptRepository.GetAttemptCountAsync(attemptId);
        tempDetails.SavedAnswers = _mapper.Map<List<SavedAnswerDto>>(attemptAggregate.Answers);
        tempDetails.Questions = _mapper.Map<List<QuestionDto>>(exam.Questions);

        if (exam.AnswerOptions is not null)
        {
            foreach (var attemptQuestion in tempDetails.Questions)
            {
                var optionsForQuestion = exam.AnswerOptions
                    .Where(opt => opt.QuestionId == attemptQuestion.Id)
                    .ToList();

                if (optionsForQuestion.Any())
                    attemptQuestion.Options = _mapper.Map<List<OptionDto>>(optionsForQuestion);
            }
        }

        var attemptResultDto = _mapper.Map<AttemptResultDto>(resultModel);

        if (attemptAggregate.Answers is not null && attemptResultDto.Details is not null)
        {
            foreach (var resultDetail in attemptResultDto.Details)
            {
                var studentAnswer = attemptAggregate.Answers
                    .FirstOrDefault(a => a.QuestionId == resultDetail.QuestionId);

                if (studentAnswer is not null && !string.IsNullOrWhiteSpace(studentAnswer.FreeTextAnswer))
                {
                    resultDetail.FreeTextAnswer = studentAnswer.FreeTextAnswer;
                }
            }
        }

        return new AttemptOverviewAggregateDto
        {
            ExamTitle = exam.Exam.Title,
            ExamDescription = exam.Exam.Description,
            AttemptResult = attemptResultDto,
            AttemptDetails = tempDetails
        };
    }

    public async Task<AttemptResultRecordDto> GetResultRecordAsync(Guid attemptResultId)
    {
        await EnsureUserHasAccessToAttemptResultAsync(attemptResultId);

        var savedResult = await _attemptResultRepository.GetRecordByIdAsync(attemptResultId);
        if (savedResult is null)
            throw new KeyNotFoundException($"Can't find attempt result with id {attemptResultId}.");

        return savedResult;
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

        var attemptDuration = attempt.FinishedAt - attempt.StartedAt;

        var result = new AttemptResultModel
        {
            Id = Uuid.NewDatabaseFriendly(Database.SqlServer),
            AttemptId = attempt.Id,
            ExamId = exam.Exam.Id,
            CourseId = exam.Exam.CourseId,
            UserId = attempt.UserId,
            TotalQuestions = exam.Questions.Count,
            Duration = attemptDuration,
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
                questionReview.AttemptId = attempt.Id;
                questionReview.AttemptResultId = result.Id;
            }
            else
            {
                var manualReview = allManualReviews.FirstOrDefault(r => r.QuestionId == question.Id);

                if (manualReview is null)
                {
                    questionReview = new QuestionResultModel
                    {
                        Id = Uuid.NewDatabaseFriendly(Database.SqlServer),
                        AttemptId = attempt.Id,
                        AttemptResultId = result.Id,
                        QuestionId = question.Id,
                        Text = question.Text,
                        SelectedOptionIds = new List<Guid>(),
                        CorrectOptionIds = new List<Guid>(),
                        Score = 0,
                        MaxScore = question.ScoreWeight,
                        IsCorrect = false,
                        QuestionResultStatus = AttemptStatus.PendingManualReview,
                    };
                }
                else
                {
                    questionReview = _mapper.Map<QuestionResultModel>(manualReview);

                    questionReview.AttemptResultId = result.Id;
                    questionReview.AttemptId = attempt.Id;
                    questionReview.MaxScore = question.ScoreWeight;

                    questionReview.QuestionResultStatus = AttemptStatus.PendingManualReview;
                }
            }

            result.Details.Add(questionReview);
        }

        result.EvaluatedOn = DateTime.UtcNow;
        result.MaxScore = result.Details.Sum(d => d.MaxScore);

        var (updatedResult, hasPending) = UpdateAttemptResultMetrics(result, exam);

        await _attemptResultRepository.SaveAttemptResultAsync(attempt.Id, updatedResult);

        attempt.AttemptResultId = updatedResult.Id;
        attempt.Status = hasPending ? AttemptStatus.PendingManualReview : AttemptStatus.Checked;


        await _attemptRepository.UpdateAsync(attempt);

        if (!hasPending)
            await _bloomService.AnalyzeAndSaveAttemptAsync(updatedResult.Id);

        return updatedResult;
    }

    private async Task<QuestionResultModel> CheckCorrectAnswersForQuestionAsync(
        Guid attemptId, Guid questionId,
        ExamAggregateModel exam)
    {
        var question = exam.Questions.SingleOrDefault(q => q.Id.Equals(questionId));
        if (question is null)
            throw new InvalidOperationException($"Question {questionId} not found in exam.");

        var userAnswers = await _examRepository.GetUserAnswersForQuestionAsync(attemptId, questionId);
        var selectedOptionIds = userAnswers.SelectMany(ua => ua.SelectedOptionIds).Distinct().ToList();

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
            MaxScore = question.ScoreWeight,
            Score = 0,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };

        switch (question.Type)
        {
            case QuestionType.SingleChoice:
                result.IsCorrect = selectedOptionIds.Count == 1 &&
                                   selectedOptionIds.First() == correctOptionIds.First();
                result.Score = result.IsCorrect ? question.ScoreWeight : 0;
                break;

            case QuestionType.MultipleChoice:
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

            case QuestionType.OpenAnswer:
                var userText = userAnswers.FirstOrDefault()?.FreeTextAnswer?.Trim();

                var isMatch = correctOptions.Any(ao =>
                    string.Equals(ao.Text.Trim(), userText, StringComparison.OrdinalIgnoreCase));

                result.IsCorrect = isMatch;
                result.Score = result.IsCorrect ? question.ScoreWeight : 0;
                break;
        }

        return result;
    }

    public async Task<AttemptResultDto> CheckOpenTextAnswersAsync(Guid attemptResultId,
        IReadOnlyList<TeacherEvaluationDto> teacherEvaluations)
    {
        var validationResults =
            await Task.WhenAll(teacherEvaluations
                .Select(a => _teacherEvaluationValidator.ValidateAsync(a)));

        if (!validationResults.All(r => r.IsValid))
            throw new ValidationException(validationResults.SelectMany(r => r.Errors));

        var attemptResult = await _attemptResultRepository.GetByIdAsync(attemptResultId);
        if (attemptResult is null)
            throw new KeyNotFoundException($"Can't find attempt result with id {attemptResultId}.");

        var examId = attemptResult.ExamId;

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

        updatedResult.EvaluatedOn = DateTime.UtcNow;

        await _attemptResultRepository.UpdateAsync(updatedResult);

        if (!hasPending)
        {
            var attempt = await _attemptRepository.GetByIdAsync(updatedResult.AttemptId);
            if (attempt is not null)
            {
                attempt.Status = AttemptStatus.Checked;
                await _attemptRepository.UpdateAsync(attempt);
                await _bloomService.AnalyzeAndSaveAttemptAsync(attemptResultId);
            }

            await _bloomService.AnalyzeAndSaveAttemptAsync(updatedResult.Id);
        }

        return _mapper.Map<AttemptResultDto>(updatedResult);
    }

    public async Task<AttemptResultDto> CheckOpenTextAnswerAsync(Guid attemptResultId,
        TeacherEvaluationDto evaluation)
    {
        var validationResults = await _teacherEvaluationValidator.ValidateAsync(evaluation);

        if (!validationResults.IsValid)
            throw new ValidationException(validationResults.Errors);

        var attemptResult = await _attemptResultRepository.GetByIdWithDetailsAsync(attemptResultId);
        if (attemptResult is null)
            throw new KeyNotFoundException($"Can't find attempt result with id {attemptResultId}.");

        var examId = attemptResult.ExamId;

        var exam = await _examRepository.GetWithQuestionsAsync(examId);
        if (exam is null)
            throw new KeyNotFoundException($"Can't find exam with id {examId}.");

        var user = _currentUserService.GetUserId();

        if (exam.Exam.AuthorId != user)
            throw new UnauthorizedAccessException("You are not allowed to perform this action.");

        var questionResult = attemptResult.Details.FirstOrDefault(q => q.QuestionId == evaluation.QuestionId);
        if (questionResult is null)
            throw new KeyNotFoundException(
                $"Can't find answer for question {evaluation.QuestionId} in this attempt result {attemptResultId}.");

        var questionDefinition = exam.Questions.First(q => q.Id == evaluation.QuestionId);

        var finalScore = Math.Min(evaluation.AwardedScore, questionDefinition.ScoreWeight);

        questionResult.Score = finalScore;
        questionResult.IsCorrect = finalScore > 0;
        questionResult.Comment = evaluation.Comment;
        questionResult.QuestionResultStatus = AttemptStatus.Checked;

        var (updatedResult, hasPending) = UpdateAttemptResultMetrics(attemptResult, exam);

        updatedResult.EvaluatedOn = DateTime.UtcNow;

        await _attemptResultRepository.UpdateAsync(updatedResult);

        if (!hasPending)
        {
            var attempt = await _attemptRepository.GetByIdAsync(updatedResult.AttemptId);
            if (attempt is not null)
            {
                attempt.Status = AttemptStatus.Checked;
                await _attemptRepository.UpdateAsync(attempt);
                
                await _bloomService.AnalyzeAndSaveAttemptAsync(attemptResultId);
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

    public async Task<IReadOnlyList<AttemptResultRecordDto>> GetUserExamResultsAttempts(Guid userId, Guid examId)
    {
        await EnsureUserHasAccessToUserAttemptsAsync(userId, examId);

        var attempts = await _attemptResultRepository.GetAllAttemptsResultsByUserByExamAsync(userId, examId);

        return _mapper.Map<List<AttemptResultRecordDto>>(attempts);
    }

    public async Task<IReadOnlyList<ExamAttemptDto>> GetUserExamAttempts(Guid userId, Guid examId)
    {
        await EnsureUserHasAccessToUserAttemptsAsync(userId, examId);

        var attempts = await _attemptRepository.GetAttemptsByUserByExamIdAsync(userId, examId);

        return _mapper.Map<List<ExamAttemptDto>>(attempts);
    }

    public async Task<IReadOnlyList<ExamAttemptDto>> GetExamAttempts(Guid examId)
    {
        await EnsureUserIsExamAuthorOrAdminAsync(examId);

        var attempts = await _attemptRepository.GetByExamAsync(examId);

        var attemptDtos = _mapper.Map<List<ExamAttemptDto>>(attempts);

        var userIds = attempts.Select(a => a.UserId).Distinct().ToList();

        var usersInfo = await _userService.GetUserBaseInformationByIdsAsync(userIds);

        foreach (var dto in attemptDtos)
        {
            var userInfo = usersInfo.FirstOrDefault(u => u.Id == dto.UserId);
            if (userInfo is not null)
            {
                dto.FirstName = userInfo.FirstName;
                dto.MiddleName = userInfo.MiddleName;
                dto.LastName = userInfo.LastName;
                dto.UserGroup = userInfo.Group;
            }
        }

        return attemptDtos;
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
            throw new UnauthorizedAccessException(
                "Only the student, the course author, or admins can view this result.");
    }

    private async Task EnsureUserHasAccessToUserAttemptsAsync(Guid requestedUserId, Guid examId)
    {
        if (_currentUserService.IsAdmin())
            return;

        var currentUserId = _currentUserService.GetUserId();

        if (currentUserId == requestedUserId)
            return;

        var isAuthor = await _examRepository.IsExamAuthorAsync(examId, currentUserId);
        if (!isAuthor)
            throw new UnauthorizedAccessException(
                "Only the student, the course author, or admins can view these attempts.");
    }

    private async Task EnsureUserIsExamAuthorOrAdminAsync(Guid examId)
    {
        if (_currentUserService.IsAdmin()) return;

        var currentUserId = _currentUserService.GetUserId();
        var isAuthor = await _examRepository.IsExamAuthorAsync(examId, currentUserId);

        if (!isAuthor)
            throw new UnauthorizedAccessException(
                "Only the course author or admins can view all attempts for this exam.");
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

        var hasPending = exam.Questions
            .Where(q => q.CheckingType == CheckingType.Manual)
            .Any(q => 
            {
                var detail = attemptResult.Details.FirstOrDefault(d => d.QuestionId == q.Id);
        
                return detail is not { QuestionResultStatus: AttemptStatus.Checked };
            });

        attemptResult.Passed = !hasPending &&
                               (exam.Exam.MinimalPassScore is null ||
                                attemptResult.Score >= exam.Exam.MinimalPassScore.Value);


        attemptResult.UpdatedOn = DateTime.UtcNow;

        return (attemptResult, hasPending);
    }
}
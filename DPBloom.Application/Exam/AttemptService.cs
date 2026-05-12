using AutoMapper;
using DPBloom.Application.Exam.Contracts;
using DPBloom.Core.Exam;
using DPBloom.Core.Exam.Enums;
using TestOfTesting.Models.Enums;
using Type = TestOfTesting.Models.Enums.Type;

namespace DPBloom.Application.Exam;

public class AttemptService : IAttemptService
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly IAttemptResultRepository _attemptResultRepository;
    private readonly IExamRepository _examRepository;
    private readonly IExamService _examService;
    private readonly IMapper _mapper;

    public AttemptService(IAttemptRepository attemptRepository, IMapper mapper, IExamRepository examRepository,
        IExamService examService, IAttemptResultRepository attemptResultRepository)
    {
        _attemptResultRepository = attemptResultRepository;
        _attemptRepository = attemptRepository;
        _examRepository = examRepository;
        _examService = examService;
        _mapper = mapper;
    }

    public async Task<Guid> StartAsync(Guid userId, Guid examId)
    {
        //TODO: add checking, if user CAN start the test by applying on course
        var exam = await _examService.GetEntityByIdAsync(examId);

        if (exam.Exam.StartsAt > DateTime.UtcNow || exam.Exam.FinishesAt < DateTime.UtcNow)
            throw new InvalidOperationException("Exam is not available for start");

        var userAttempt = new UserExamAttemptModel
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExamId = examId,
            Status = AttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            IsValid = true,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };
        var startedModel = await _attemptRepository.StartAsync(userAttempt);

        return startedModel.Id;
    }

    public async Task SubmitAnswerAsync(Guid userId, Guid attemptId, SubmitAnswerDto answer)
    {
        //TODO: add submit answer validator

        // var validation...

        // 1. Отримуємо спробу з бази даних
        // (У тебе в IAttemptRepository має бути метод для отримання спроби)
        var attempt = await _attemptRepository.GetByIdAsync(attemptId);

        // 2. Перевіряємо, чи існує така спроба взагалі
        if (attempt == null)
        {
            throw new KeyNotFoundException("Спробу не знайдено.");
        }

        // 3. БІЗНЕС-ПЕРЕВІРКА ВЛАСНИКА (Authorization)
        if (attempt.UserId != userId)
        {
            // Кидаємо виняток. Сервіс не знає про HTTP 403, він просто каже "Доступ заборонено"
            throw new UnauthorizedAccessException("Ви не маєте доступу до цієї спроби.");
        }

        // 4. (Опціонально) Можна додати перевірку, чи спроба ще активна
        // if (attempt.Status != AttemptStatus.InProgress)
        //     throw new InvalidOperationException("Ця спроба вже завершена.");

        // 5. Валідація DTO (FluentValidation)
        // TODO: add submit answer validator

        // 6. Мапінг і збереження
        var answerModel = _mapper.Map<UserAnswerModel>(answer);
        answerModel.Id = Guid.NewGuid();
        answerModel.CreatedOn = answerModel.UpdatedOn = DateTime.UtcNow;
        answerModel.SubmittedAt = DateTime.UtcNow;

        // Зверни увагу: використовуємо attemptId з параметрів методу (маршруту), 
        // а не з тіла запиту (answer.AttemptId), щоб уникнути підміни даних хакером.
        await _attemptRepository.SubmitAnswerAsync(attemptId, answerModel);
    }

    public async Task SaveAllAnswersAsync(Guid attemptId, List<SubmitAnswerDto> answers)
    {
        //TODO: add submit answer validator

        // var validation...

        var answerModels = _mapper.Map<List<UserAnswerModel>>(answers);

        foreach (var answerModel in answerModels)
        {
            answerModel.Id = Guid.NewGuid();
            answerModel.CreatedOn = answerModel.UpdatedOn = DateTime.UtcNow;
            answerModel.SubmittedAt = DateTime.UtcNow;
        }

        await _attemptRepository.SaveAllAnswersAsync(attemptId, answerModels);
    }

    public async Task<AttemptResultDto> FinishAsync(Guid userId, Guid attemptId)
    {
        var attempt = await GetEntityByIdAsync(attemptId);

        if (attempt is null)
            throw new KeyNotFoundException("Спробу не знайдено.");

        if (attempt.UserId != userId)
            throw new UnauthorizedAccessException("Ви не маєте доступу до цієї спроби.");

        var exam = await _examRepository.GetWithQuestionsAsync(attempt.ExamId);

        if (attempt.Status is AttemptStatus.Submitted or AttemptStatus.Expired)
        {
            return await GetResultAsync(userId, attemptId);
            ;
        }

        attempt.FinishedAt = DateTime.UtcNow;

        if (attempt.FinishedAt > exam.Exam.FinishesAt || attempt.FinishedAt - attempt.StartedAt > exam.Exam.Duration)
            attempt.Status = AttemptStatus.Expired;
        else
            attempt.Status = AttemptStatus.Submitted;

        await _attemptRepository.FinishAsync(attempt);

        /*try
        {
            await CalculateAndSaveResultAsync(attempt, exam);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during automatic result calculation for attempt {attemptId}: {ex.Message}");
        }*/

        var resultModel = await CalculateAndSaveResultAsync(attempt, exam);
        return _mapper.Map<AttemptResultDto>(resultModel);
    }

    public async Task<AttemptResultDto> GetResultAsync(Guid userId, Guid attemptId)
    {
        var attempt = await GetEntityByIdAsync(attemptId);

        if (attempt is null)
            throw new KeyNotFoundException("Спробу не знайдено.");

        if (attempt.UserId != userId)
            throw new UnauthorizedAccessException("Ви не маєте доступу до цієї спроби."); //TODO: maybe add validation?

        if (attempt.AttemptResultId.HasValue)
        {
            var savedResult = await _attemptResultRepository.GetByIdAsync(attempt.AttemptResultId.Value);
            return _mapper.Map<AttemptResultDto>(savedResult);
        }

        var exam = await _examRepository.GetWithQuestionsAsync(attempt.ExamId);
        var resultModel = await CalculateAndSaveResultAsync(attempt, exam);
        return _mapper.Map<AttemptResultDto>(resultModel);
    }

    public async Task<UserExamAttemptModel> GetEntityByIdAsync(Guid id)
    {
        var examAttempt = await _attemptRepository.GetByIdAsync(id);

        if (examAttempt is null)
            throw new KeyNotFoundException($"Lecture with ID {id} not found");

        return examAttempt;
    }

    private async Task<AttemptResultModel> CalculateAndSaveResultAsync(UserExamAttemptModel attempt,
        ExamAggregateModel exam)
    {
        if (attempt.AttemptResultId.HasValue)
        {
            return await _attemptResultRepository.GetByIdAsync(attempt.AttemptResultId.Value);
        }

        var result = new AttemptResultDto
        {
            AttemptId = attempt.Id,
            ExamId = exam.Exam.Id,
            TotalQuestions = exam.Questions.Count,
            Details = []
        };

        var correctAnswers = 0;
        var totalPossibleScore = exam.Questions.Select(q => q.ScoreWeight).Sum();
        var score = 0.0;

        var tasks = exam.Questions.Select(async question =>
        {
            QuestionResultDto questionReview;

            if (question.CheckingType == CheckingType.Automatic)
            {
                questionReview = await CheckCorrectAnswersForQuestionAsync(attempt.Id, question.Id, exam);
            }
            else
            {
                var manualReview = await _attemptRepository.FindManuallyReviewedAnswer(attempt.Id, question.Id);
                if (manualReview is null)
                    throw new InvalidOperationException("Cannot find manual review for this question");

                questionReview = _mapper.Map<QuestionResultDto>(manualReview);
            }

            return questionReview;
        });

        var questionReviews = await Task.WhenAll(tasks);

        foreach (var r in questionReviews)
        {
            result.Details.Add(r);
            score += r.Score;

            if (r.IsCorrect)
                correctAnswers++;
        }

        result.Score = score;
        result.CorrectAnswers = correctAnswers;
        result.ScorePercentage = totalPossibleScore > 0
            ? Math.Round((score / totalPossibleScore) * 100, 2)
            : 0;
        result.Passed = exam.Exam.MinimalPassScore is null
                        || result.Score >= exam.Exam.MinimalPassScore.Value;

        var resultToSave = _mapper.Map<AttemptResultModel>(result);

        await _attemptResultRepository.SaveAttemptResultAsync(attempt.Id, resultToSave);

        attempt.AttemptResultId = resultToSave.Id;
        attempt.Status =
            AttemptStatus
                .Checked; //TODO add checking status for unvalidated attempts (for example, if teacher had to check it manually)

        await _attemptRepository.UpdateAsync(attempt);

        return resultToSave;
    }

    private async Task<QuestionResultDto> CheckCorrectAnswersForQuestionAsync(Guid attemptId, Guid questionId,
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

        var correctOptionIds = exam.AnswerOptions
            .Where(ao => ao.QuestionId.Equals(questionId) && ao.IsCorrect)
            .Select(ao => ao.Id)
            .ToList();

        var result = new QuestionResultDto
        {
            QuestionId = questionId,
            Text = question.Text,
            SelectedOptionIds = selectedOptionIds,
            CorrectOptionIds = correctOptionIds,
            Score = 0
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

                var score = correctSelected * weightPerCorrect;

                var isFullyCorrect = correctSelected == correctCount && incorrectSelected == 0;

                if (isFullyCorrect)
                {
                    score = question.ScoreWeight;
                }

                result.Score = score;
                result.IsCorrect = isFullyCorrect;
                break;

            case Type.OpenAnswer:
                var correctText = correctOptions.Select(ao => ao.Text.ToLower()).ToList();
                var userText = userAnswers.Select(ut => ut.FreeTextAnswer.ToLower()).ToList();

                result.IsCorrect = !correctText.Except(userText).Any() && !userText.Except(correctText).Any();
                result.Score = result.IsCorrect ? question.ScoreWeight : 0;
                break;
        }

        return result;
    }
}
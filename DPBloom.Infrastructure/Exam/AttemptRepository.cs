using AutoMapper;
using DPBloom.Application.Attempt;
using DPBloom.Application.Attempt.Contracts;
using DPBloom.Core.Exam;
using DPBloom.Core.Exam.Enums;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Exam;

public class AttemptRepository : RepositoryBase<UserExamAttemptModel, UserExamAttemptDao, ApplicationDbContext>,
    IAttemptRepository
{
    public AttemptRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task<UserExamAttemptModel> GetWithAnswersAsync(Guid attemptId)
    {
        var userExamAttempt = await DbContext.UserExamAttempts
            .Include(a => a.Answers)
            .ThenInclude(ans => ans.Question)
            .FirstOrDefaultAsync(a => a.Id.Equals(attemptId));

        return Mapper.Map<UserExamAttemptModel>(userExamAttempt);
    }

    public async Task<IReadOnlyList<UserExamAttemptModel>> GetByUserAsync(Guid userId)
    {
        var examAttempts = await DbContext.UserExamAttempts
            .Where(a => a.UserId.Equals(userId))
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();

        return Mapper.Map<List<UserExamAttemptModel>>(examAttempts);
    }

    public async Task<IReadOnlyList<UserExamAttemptModel>> GetByExamAsync(Guid examId)
    {
        var examAttempts = await DbContext.UserExamAttempts
            .Where(a => a.ExamId.Equals(examId))
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();

        return Mapper.Map<List<UserExamAttemptModel>>(examAttempts);
    }

    public async Task<UserExamAttemptModel> StartAsync(UserExamAttemptModel model)
    {
        var startAttempt = Mapper.Map<UserExamAttemptDao>(model);

        DbContext.UserExamAttempts.Add(startAttempt);

        await DbContext.SaveChangesAsync();

        return Mapper.Map<UserExamAttemptModel>(startAttempt);
    }

    public async Task SubmitAnswerAsync(Guid attemptId, UserQuestionAnswerModel questionAnswer)
    {
        var answerEntity = Mapper.Map<UserQuestionAnswerDao>(questionAnswer);

        DbContext.UserAnswers.Add(answerEntity);

        await DbContext.SaveChangesAsync();
    }

    public async Task SaveAllAnswersAsync(Guid attemptId, List<UserQuestionAnswerModel> answers)
    {
        var answerEntities = Mapper.Map<List<UserQuestionAnswerDao>>(answers);

        await DbContext.UserAnswers.AddRangeAsync(answerEntities);

        await DbContext.SaveChangesAsync();
    }

    public async Task FinishAsync(UserExamAttemptModel model)
    {
        var attempt = Mapper.Map<UserExamAttemptDao>(model);

        DbContext.UserExamAttempts.Update(attempt);

        await DbContext.SaveChangesAsync();
    }

    public async Task<QuestionResultModel> FindManuallyReviewedAnswer(Guid attemptId, Guid questionId)
    {
        var result = await DbContext.QuestionResults.FirstOrDefaultAsync(qr =>
            qr.QuestionId.Equals(questionId) && qr.AttemptId.Equals(attemptId));

        return Mapper.Map<QuestionResultModel>(result);
    }

    public async Task<Guid?> GetCourseIdByAttemptIdAsync(Guid attemptId)
    {
        return await DbContext.Exams
            .Where(l => l.Id == attemptId)
            .Select(l => l.CourseId)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<ActiveAttemptInfoDto>> GetActiveAttemptsInfoAsync(int batchSize)
    {
        return await DbContext.Set<UserExamAttemptDao>()
            .Where(a => !a.IsDeleted && a.Status == AttemptStatus.InProgress)
            .OrderBy(a => a.StartedAt)
            .Take(batchSize)
            .Select(a => new ActiveAttemptInfoDto
            {
                AttemptId = a.Id,
                StartedAt = a.StartedAt,
                Duration = a.Exam.Duration
            })
            .ToListAsync();
    }

    public async Task CloseAttemptsAsync(List<Guid> attemptIds)
    {
        if (!attemptIds.Any()) return;

        var now = DateTime.UtcNow;

        await DbContext.Set<UserExamAttemptDao>()
            .Where(a => attemptIds.Contains(a.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.Status, AttemptStatus.Expired)
                .SetProperty(a => a.FinishedAt, now));
    }

    public async Task CloseAllExpiredAttemptsAsync()
    {
        var now = DateTime.UtcNow;

        await DbContext.UserExamAttempts
            .Where(a => a.Status == AttemptStatus.InProgress &&
                        (a.StartedAt.Add(a.Exam.Duration) < now))
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.Status, AttemptStatus.Expired)
                .SetProperty(a => a.FinishedAt, now));
    }

    public async Task<IReadOnlyList<ExamAttemptDto>> GetAttemptsByUserByExamIdAsync(Guid userId, Guid examId)
    {
        var attempts = await DbContext.UserExamAttempts
            .AsNoTracking()
            .Where(a => a.ExamId.Equals(examId) && a.UserId.Equals(userId))
            .ToListAsync();

        return Mapper.Map<List<ExamAttemptDto>>(attempts);
    }

    public async Task SaveResultAndUpdateAttemptAsync(UserExamAttemptModel attemptModel, AttemptResultModel resultModel)
    {
        var attemptDao = Mapper.Map<UserExamAttemptDao>(attemptModel);
        var resultDao = Mapper.Map<AttemptResultDao>(resultModel);

        attemptDao.AttemptResult = resultDao;

        DbContext.UserExamAttempts.Update(attemptDao);

        await DbContext.SaveChangesAsync();
    }

    public async Task SaveFullAttemptResultAsync(UserExamAttemptModel attemptModel, AttemptResultModel resultModel)
    {
        var attemptDao = Mapper.Map<UserExamAttemptDao>(attemptModel);
        var resultDao = Mapper.Map<AttemptResultDao>(resultModel);

        attemptDao.AttemptResult = resultDao;

        foreach (var detail in resultDao.Details)
        {
            detail.AttemptResult = resultDao;
        }

        DbContext.UserExamAttempts.Update(attemptDao);

        await DbContext.SaveChangesAsync();
    }

    public async Task<UserExamAttemptModel?> GetAttemptWithFullDetailsAsync(Guid attemptId)
    {
        var attemptDao = await DbContext.UserExamAttempts
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Exam)
            .ThenInclude(e => e.Questions)
            .ThenInclude(q => q.Options)
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        return attemptDao == null ? null : Mapper.Map<UserExamAttemptModel>(attemptDao);
    }

    public async Task<IReadOnlyList<UserQuestionAnswerModel>> GetAnswersByAttemptIdAsync(Guid attemptId)
    {
        var answersDao = await DbContext.UserAnswers
            .AsNoTracking()
            .Where(a => a.AttemptId == attemptId)
            .ToListAsync();

        return Mapper.Map<List<UserQuestionAnswerModel>>(answersDao);
    }

    public async Task<UserQuestionAnswerModel?> GetAnswerAsync(Guid attemptId, Guid questionId)
    {
        var dao = await DbContext.UserAnswers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.QuestionId == questionId);

        return dao == null ? null : Mapper.Map<UserQuestionAnswerModel>(dao);
    }

    public async Task AddAnswerAsync(UserQuestionAnswerModel questionAnswer)
    {
        var answerEntity = Mapper.Map<UserQuestionAnswerDao>(questionAnswer);
        DbContext.UserAnswers.Add(answerEntity);
        await DbContext.SaveChangesAsync();
    }

    public async Task UpdateAnswerAsync(UserQuestionAnswerModel questionAnswer)
    {
        var answerEntity = Mapper.Map<UserQuestionAnswerDao>(questionAnswer);
        DbContext.UserAnswers.Update(answerEntity);
        await DbContext.SaveChangesAsync();
    }

    public async Task<Guid?> GetActiveAttemptIdAsync(Guid userId, Guid examId)
    {
        var activeAttemptId = await DbContext.UserExamAttempts
            .AsNoTracking()
            .Where(a => a.UserId == userId
                        && a.ExamId == examId
                        && a.Status == AttemptStatus.InProgress)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();

        return activeAttemptId == Guid.Empty ? null : activeAttemptId;
    }

    public async Task SaveAnswersBatchAsync(List<UserQuestionAnswerModel> newAnswers,
        List<UserQuestionAnswerModel> existingAnswersToUpdate)
    {
        if (newAnswers.Any())
        {
            var entitiesToAdd = Mapper.Map<List<UserQuestionAnswerDao>>(newAnswers);
            await DbContext.UserAnswers.AddRangeAsync(entitiesToAdd);
        }

        if (existingAnswersToUpdate.Any())
        {
            var entitiesToUpdate = Mapper.Map<List<UserQuestionAnswerDao>>(existingAnswersToUpdate);
            DbContext.UserAnswers.UpdateRange(entitiesToUpdate);
        }

        await DbContext.SaveChangesAsync();
    }
    
    public async Task<UserExamAttemptAggregateModel?> GetAttemptAggregateAsync(Guid attemptId)
    {
        var attemptDao = await DbContext.UserExamAttempts
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Exam)
            .ThenInclude(e => e.Questions)
            .ThenInclude(q => q.Options)
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        if (attemptDao == null) return null;

        return new UserExamAttemptAggregateModel
        {
            Attempt = Mapper.Map<UserExamAttemptModel>(attemptDao),
        
            Exam = attemptDao.Exam is not null ? Mapper.Map<ExamModel>(attemptDao.Exam) : null,
            Questions = attemptDao.Exam != null ? Mapper.Map<List<QuestionModel>>(attemptDao.Exam.Questions) : [],
        
            Answers = Mapper.Map<List<UserQuestionAnswerModel>>(attemptDao.Answers)
        };
    }

    public async Task<int> GetAttemptCountAsync(Guid attemptId)
    {
        var attempt = await DbContext.UserExamAttempts.FirstOrDefaultAsync(uea => uea.Id == attemptId);

        var count = DbContext.UserExamAttempts.Count(a =>
            a.UserId == attempt.UserId && a.ExamId == attempt.ExamId && a.StartedAt <= attempt.StartedAt);
        
        return count;
    }
}
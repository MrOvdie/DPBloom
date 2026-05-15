using System.Linq.Dynamic.Core;
using AutoMapper;
using DPBloom.Application.Exam;
using DPBloom.Application.Exam.Contracts;
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

    public async Task<List<UserExamAttemptModel>> GetByUserAsync(Guid userId)
    {
        var examAttempts = await DbContext.UserExamAttempts
            .Where(a => a.UserId.Equals(userId))
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();
        
        return Mapper.Map<List<UserExamAttemptModel>>(examAttempts);
    }

    public async Task<List<UserExamAttemptModel>> GetByExamAsync(Guid examId)
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
        
        await DbContext.SaveChangesAsync(); //TODO: maybe add some checking confirmation, if entity was updated
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
        var result = await DbContext.QuestionResults.FirstOrDefaultAsync(qr => qr.QuestionId.Equals(questionId) && qr.AttemptId.Equals(attemptId));
        
        return Mapper.Map<QuestionResultModel>(result);
    }
    
    public async Task<Guid?> GetCourseIdByAttemptIdAsync(Guid attemptId)
    {
        return await DbContext.Exams
            .Where(l => l.Id == attemptId)
            .Select(l => l.CourseId)
            .FirstOrDefaultAsync();
    }
    
    public async Task<List<ActiveAttemptInfoDto>> GetActiveAttemptsInfoAsync(int batchSize)
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
}
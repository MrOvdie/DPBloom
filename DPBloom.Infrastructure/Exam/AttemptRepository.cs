using AutoMapper;
using DPBloom.Core.Exam;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TestOfTesting.Models.Enums;

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

    public async Task<UserExamAttemptModel> StartAsync(UserExamAttemptModel model)
    {
       var startAttempt = Mapper.Map<UserExamAttemptDao>(model);

        DbContext.UserExamAttempts.Add(startAttempt);
        
        await DbContext.SaveChangesAsync();

        return Mapper.Map<UserExamAttemptModel>(startAttempt);
    }

    public async Task SubmitAnswerAsync(Guid attemptId, UserAnswerModel answer)
    {
        var answerEntity = Mapper.Map<UserAnswerDao>(answer);

        DbContext.UserAnswers.Add(answerEntity);
        
        await DbContext.SaveChangesAsync(); //TODO: maybe add some checking confirmation, if entity was updated
    }

    public async Task SaveAllAnswersAsync(Guid attemptId, List<UserAnswerModel> answers)
    {
        var answerEntities = Mapper.Map<List<UserAnswerDao>>(answers);
        
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
}
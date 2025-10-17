using System.Data.Entity;
using AutoMapper;
using DPBloom.Core.Exam;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Exam;

public class AttemptResultRepository : RepositoryBase<AttemptResultModel, UserExamAttemptDao, ApplicationDbContext>,
    IAttemptResultRepository
{
    public AttemptResultRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
    }

    public async Task SaveManualQuestionAnswerReviewAsync(Guid attemptResultId, QuestionResultModel model)
    {
        //TODO: check if this is correct approach
        var existingAttemptResult = await DbContext.AttemptResults
            .FirstOrDefaultAsync(qr => qr.Id.Equals(attemptResultId));

        var newResult = Mapper.Map<QuestionResultDao>(model);
        existingAttemptResult.Details.Add(newResult);

        DbContext.AttemptResults.Update(existingAttemptResult);

        await DbContext.SaveChangesAsync();
    }

    public async Task SaveAttemptResultAsync(Guid attemptId, AttemptResultModel model)
    {
        await DbContext.UserExamAttempts
            .Where(a => a.Id == attemptId)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(b => b.TotalScore, model.Score));

        var attemptResultDao = Mapper.Map<AttemptResultDao>(model);
        attemptResultDao.Id = Guid.NewGuid();
        attemptResultDao.CreatedOn = attemptResultDao.UpdatedOn = DateTime.UtcNow;
        attemptResultDao.AttemptId = attemptId;

        await DbContext.AttemptResults.AddAsync(attemptResultDao);

        await DbContext.SaveChangesAsync();
    }
}
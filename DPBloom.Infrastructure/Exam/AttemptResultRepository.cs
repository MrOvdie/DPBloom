using AutoMapper;
using AutoMapper.QueryableExtensions;
using DPBloom.Application.Attempt;
using DPBloom.Application.Attempt.Contracts;
using DPBloom.Core.Exam;
using DPBloom.Core.Exam.Enums;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Exam;

public class AttemptResultRepository : RepositoryBase<AttemptResultModel, AttemptResultDao, ApplicationDbContext>,
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
        var attemptResultDao = Mapper.Map<AttemptResultDao>(model);
        
        if (attemptResultDao.Details is not null)
        {
            foreach (var detail in attemptResultDao.Details)
            {
                detail.AttemptResultId = attemptResultDao.Id;
                detail.AttemptResult = attemptResultDao;
            }
        }
        
        await DbContext.AttemptResults.AddAsync(attemptResultDao);

        await DbContext.SaveChangesAsync();
    }

    public async Task<AttemptResultModel> GetAttemptResultByIdAsync(Guid attemptResultId)
    {
        var attemptResult = await DbContext.AttemptResults.FindAsync(attemptResultId);

        return Mapper.Map<AttemptResultModel>(attemptResult);
    }

    public async Task<AttemptResultModel> GetAttemptResultByExamIdAsync(Guid examId)
    {
        var attemptResult = await DbContext.AttemptResults.FirstOrDefaultAsync(ar => ar.ExamId.Equals(examId));

        return Mapper.Map<AttemptResultModel>(attemptResult);
    }

    public async Task<IReadOnlyList<AttemptResultModel>> GetAllAttemptsResultsByUserAsync(Guid userId)
    {
        var attemptsResultsDao = await DbContext.AttemptResults
            .Include(a => a.Details)
            .Where(ar => ar.UserId.Equals(userId)).ToListAsync();

        return Mapper.Map<List<AttemptResultModel>>(attemptsResultsDao);
    }

    public async Task<IReadOnlyList<AttemptResultModel>> GetAllAttemptsResultsByUserByExamAsync(Guid userId, Guid examId)
    {
        var attemptsResultsDao = await DbContext.AttemptResults
            .Where(ar => ar.UserId.Equals(userId) && ar.ExamId.Equals(examId)).ToListAsync();

        return Mapper.Map<List<AttemptResultModel>>(attemptsResultsDao);
    }

    public async Task<IReadOnlyList<AttemptResultModel>> GetAllAttemptsResultsByExamAsync(Guid examId)
    {
        var attemptsResultsDao = await DbContext.AttemptResults
            .Where(ar => ar.ExamId.Equals(examId)).ToListAsync();

        return Mapper.Map<List<AttemptResultModel>>(attemptsResultsDao);
    }

    public async Task<bool> IsAttemptResultOwnerAsync(Guid attemptResultId, Guid userId)
    {
        return await DbContext.UserExamAttempts
            .AnyAsync(c => c.Id == attemptResultId && c.UserId == userId);
    }

    public async Task<bool> IsExamAuthorByAttemptResultAsync(Guid attemptResultId, Guid userId)
    {
        return await DbContext.AttemptResults
            .AnyAsync(ar =>
                ar.Id == attemptResultId &&
                ar.Exam.Course.AuthorId == userId);
    }

    public async Task<List<UserQuestionAnswerModel>> GetAllManualReviewsForAttemptAsync(Guid attemptId)
    {
        var manualReviewAnswers = await DbContext.UserAnswers
            .Where(answer => answer.AttemptId == attemptId)
            .Where(a => a.Question.CheckingType == CheckingType.Manual)
            .ToListAsync();
        return Mapper.Map<List<UserQuestionAnswerModel>>(manualReviewAnswers);
    }

    public async Task<List<AttemptResultModel>> GetAllManualReviewsAttemptsForExamAsync(Guid examId)
    {
        var manualReviewAttempts = await DbContext.AttemptResults
            .Where(ar => ar.ExamId == examId)
            .Where(ar => ar.Details.Any(ard => ard.QuestionResultStatus == AttemptStatus.PendingManualReview))
            .ToListAsync();

        return Mapper.Map<List<AttemptResultModel>>(manualReviewAttempts);
    }
    
    public async Task<AttemptAccessInfo?> GetAccessInfoAsync(Guid attemptResultId)
    {
        return await DbContext.AttemptResults
            .Where(ar => ar.Id == attemptResultId)
            .Select(ar => new AttemptAccessInfo(
                ar.Attempt.UserId, 
                ar.Exam.AuthorId)) 
            .FirstOrDefaultAsync();
    }
    
    public async Task<AttemptResultRecordDto?> GetRecordByIdAsync(Guid attemptResultId)
    {
        return await DbContext.AttemptResults
            .Where(a => a.Id == attemptResultId)
            .ProjectTo<AttemptResultRecordDto>(Mapper.ConfigurationProvider) 
            .FirstOrDefaultAsync();
    }
    
    public async Task<AttemptResultModel> GetByIdWithDetailsAsync(Guid id)
    {
        var attemptResult =  await DbContext.AttemptResults
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == id);
        
        return Mapper.Map<AttemptResultModel>(attemptResult);
    }
}
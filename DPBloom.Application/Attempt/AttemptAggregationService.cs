using DPBloom.Application.Attempt.Contracts;
using DPBloom.Application.Bloom;
using DPBloom.Application.Exam;
using DPBloom.Core.Exam.Enums;

namespace DPBloom.Application.Attempt;

public class AttemptAggregationService : IAttemptAggregationService
{
    private readonly IBloomService _bloomService;
    private readonly IAttemptService _attemptService;
    private readonly IExamRepository _examRepository;
    
    

    public AttemptAggregationService(IBloomService bloomService, IAttemptService attemptService, IExamRepository examRepository)
    {
        _bloomService = bloomService;
        _attemptService = attemptService;
        _examRepository = examRepository;
    }

    public async Task<IReadOnlyList<AttemptResultWithStatsDto>> GetAttemptResultsWithStatisticsByExamByUserAsync(Guid userId, Guid examId)
    {
        var attemptRecords = await _attemptService.GetUserExamResultsAttempts(userId, examId);
    
        if (attemptRecords is null || !attemptRecords.Any())
            return [];

        var attemptResultIds = attemptRecords.Select(ar => ar.Id).ToList();
        
        var attempts = await _attemptService.GetUserExamAttempts(userId, examId);

        var bloomStats = await _bloomService.GetAnalysisByAttemptResultIdsAsync(attemptResultIds);
    
        var aggregatedResults = attemptRecords.Select(attempt => new AttemptResultWithStatsDto
        {
            AttemptResult = attempt, 
        
            ExamAttempt = attempts.FirstOrDefault(a => a.Id.Equals(attempt.AttemptId)),
            
            BloomAnalytics = bloomStats.FirstOrDefault(b => b.AttemptResultId == attempt.Id)
        }).ToList();

        return aggregatedResults;
    }

   public async Task<IReadOnlyList<AttemptResultWithStatsDto>> GetAttemptResultsWithStatisticsByExamAsync(Guid examId)
{
    var exam = await _examRepository.GetByIdAsync(examId);
    if (exam is null)
        return [];

    var attemptRecords = await _attemptService.GetAttemptResultsByExamAsync(examId);
    if (attemptRecords is null || !attemptRecords.Any())
        return [];

    var attempts = await _attemptService.GetExamAttempts(examId);

    var combinedData = attemptRecords
        .Join(attempts, 
            result => result.AttemptId, 
            attempt => attempt.Id, 
            (result, attempt) => new { AttemptResult = result, ExamAttempt = attempt })
        .ToList();

    var processedAttemptsData = combinedData
        .GroupBy(x => x.ExamAttempt.UserId)
        .Select(group => 
        {
            var hasUnchecked = group.Any(x => x.ExamAttempt.Status != AttemptStatus.Checked); 
            
            var targetAttempt = exam.EvaluationStrategy == EvaluationStrategy.Last
                ? group.OrderByDescending(x => x.ExamAttempt.StartedAt).First()
                : group.OrderByDescending(x => x.AttemptResult.Score).First();

            return new 
            { 
                TargetData = targetAttempt, 
                HasUncheckedAttempts = hasUnchecked 
            };
        })
        .ToList();

    var targetAttemptResultIds = processedAttemptsData
        .Select(x => x.TargetData.AttemptResult.Id)
        .ToList();

    var bloomStats = await _bloomService.GetAnalysisByAttemptResultIdsAsync(targetAttemptResultIds);

    var aggregatedResults = processedAttemptsData.Select(data => new AttemptResultWithStatsDto
    {
        AttemptResult = data.TargetData.AttemptResult, 
        ExamAttempt = data.TargetData.ExamAttempt,
        BloomAnalytics = bloomStats.FirstOrDefault(b => b.AttemptResultId == data.TargetData.AttemptResult.Id),
        
        HasUncheckedAttempts = data.HasUncheckedAttempts 
    }).ToList();

    return aggregatedResults;
}

    public async Task<AttemptResultWithStatsDto> GetAttemptResultsWithStatisticsByIdAsync(Guid attemptResultId)
    {
        var attemptResult = await _attemptService.GetResultRecordAsync(attemptResultId);
        if (attemptResult is null)
            throw new KeyNotFoundException("Attempts not found");
        
        var bloomStats = await _bloomService.GetAnalysisByAttemptResultIdAsync(attemptResultId);
        if (bloomStats is null)
            throw new KeyNotFoundException("Stats not found");
        
        return new AttemptResultWithStatsDto
        {
            AttemptResult = attemptResult,
            BloomAnalytics = bloomStats
        };
    }
}
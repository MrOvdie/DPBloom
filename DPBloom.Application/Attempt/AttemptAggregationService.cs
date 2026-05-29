using DPBloom.Application.Attempt.Contracts;
using DPBloom.Application.Bloom;

namespace DPBloom.Application.Attempt;

public class AttemptAggregationService : IAttemptAggregationService
{
    private readonly IBloomService _bloomService;
    private readonly IAttemptService _attemptService;
    
    

    public AttemptAggregationService(IBloomService bloomService, IAttemptService attemptService)
    {
        _bloomService = bloomService;
        _attemptService = attemptService;
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
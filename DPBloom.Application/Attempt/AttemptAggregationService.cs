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
        var attempts = await _attemptService.GetUserExamAttempts(userId, examId);
    
        if (attempts is null || !attempts.Any())
            return [];

        var attemptResultIds = attempts.Select(ar => ar.Id).ToList();

        var bloomStats = await _bloomService.GetAnalysisByAttemptResultIdsAsync(attemptResultIds);
    
        var aggregatedResults = attempts.Select(attempt => new AttemptResultWithStatsDto
        {
            AttemptResult = attempt, 
        
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
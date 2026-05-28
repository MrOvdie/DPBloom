using DPBloom.Application.Attempt.Contracts;

namespace DPBloom.Application.Attempt;

public interface IAttemptAggregationService
{
    Task<IReadOnlyList<AttemptResultWithStatsDto>> GetAttemptResultsWithStatisticsByExamByUserAsync(Guid userId,
        Guid examId);

    Task<AttemptResultWithStatsDto> GetAttemptResultsWithStatisticsByIdAsync(Guid attemptResultId);
}
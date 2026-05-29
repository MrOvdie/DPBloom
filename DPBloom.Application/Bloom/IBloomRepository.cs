using DPBloom.Application.Base;
using DPBloom.Core.Bloom;

namespace DPBloom.Application.Bloom;

public interface IBloomRepository : IRepository<BloomAnalysisModel>
{
    Task<BloomAnalysisModel> GetByAttemptResultIdAsync(Guid attemptResultId);
    Task<IReadOnlyList<BloomAnalysisModel>> GetByAttemptResultIdsAsync(IReadOnlyList<Guid> attemptResultIds);
    Task<BloomAnalysisModel> AddAnalysisAsync(BloomAnalysisModel analysis);
    Task<IReadOnlyList<BloomAnalysisModel>> GetAnalysisByAttemptResultIdsAsync(List<Guid> attemptResultIds);
}
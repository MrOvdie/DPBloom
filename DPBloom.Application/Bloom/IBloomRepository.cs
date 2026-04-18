using DPBloom.Application.Base;
using DPBloom.Core.Bloom;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Bloom;

namespace DPBloom.Application.Bloom;

public interface IBloomRepository : IRepository<BloomAnalysisModel>
{
    Task<BloomAnalysisModel> GetByAttemptResultIdAsync(Guid attemptResultId);
    Task<BloomAnalysisModel> AddAnalysisAsync(BloomAnalysisModel analysis);
}
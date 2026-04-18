using AutoMapper;
using DPBloom.Application.Bloom;
using DPBloom.Core.Bloom;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Bloom;

public class BloomRepository : RepositoryBase<BloomAnalysisModel, BloomAnalysisDao, ApplicationDbContext>, IBloomRepository
{
    public BloomRepository(ApplicationDbContext context, IMapper mapper) 
        : base(context, mapper)
    {
    }
    
    public async Task<BloomAnalysisModel> GetByAttemptResultIdAsync(Guid attemptResultId)
    {
        var bloomResul = await DbContext.BloomAnalyses
            .FirstOrDefaultAsync(b => b.AttemptResultId.Equals(attemptResultId));
        
        return Mapper.Map<BloomAnalysisModel>(bloomResul);
    }

    public async Task<BloomAnalysisModel> AddAnalysisAsync(BloomAnalysisModel analysis)
    {
       var analysisDao = Mapper.Map<BloomAnalysisDao>(analysis);
        
       DbContext.BloomAnalyses.Add(analysisDao);
       await DbContext.SaveChangesAsync();
       
       return Mapper.Map<BloomAnalysisModel>(analysisDao);
    }
}
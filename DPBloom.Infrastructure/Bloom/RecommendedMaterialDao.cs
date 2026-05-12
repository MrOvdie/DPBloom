using DPBloom.Infrastructure.Base;

namespace DPBloom.Infrastructure.Bloom;

public class RecommendedMaterialDao : EntityDaoBase<Guid>
{
    public Guid MaterialId { get; set; }
    public string Title { get; set; }
    public int RelevantBloomLevel { get; set; }
}
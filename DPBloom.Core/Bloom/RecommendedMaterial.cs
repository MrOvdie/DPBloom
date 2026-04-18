using TestOfTesting.Models.Enums;

namespace DPBloom.Core.Bloom;

public class RecommendedMaterial
{
    public Guid MaterialId { get; set; }
    public string Title { get; set; }
    public Category RelevantBloomLevel { get; set; }
}
using DPBloom.Core.Exam.Enums;
using TestOfTesting.Models.Enums;

namespace DPBloom.Core.Bloom;

public class RecommendedMaterial
{
    public Guid MaterialId { get; set; }
    public Guid TopicId { get; set; }
    public string Title { get; set; }
    public BloomLevel RelevantBloomLevel { get; set; }
}
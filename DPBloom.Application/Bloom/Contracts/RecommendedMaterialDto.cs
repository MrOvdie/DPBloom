namespace DPBloom.Application.Bloom.Contracts;

public class RecommendedMaterialDto
{
    public Guid MaterialId { get; set; }
    public string MaterialType { get; set; } // "Lecture", "Topic" //TODO: rethink this
    public string Title { get; set; }
    public string RelevantBloomLevelName { get; set; }
}
using DPBloom.Core.Exam.Enums;

namespace DPBloom.Application.ANN;

public interface IBloomLevelPredictor
{
    Task<BloomLevel> PredictLevelAsync(string questionText); 
}
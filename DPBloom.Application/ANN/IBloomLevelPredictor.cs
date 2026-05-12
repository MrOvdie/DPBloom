namespace DPBloom.Application.ANN;

public interface IBloomLevelPredictor
{
    Task<int> PredictLevelAsync(string questionText); 
}
using DPBloom.Application.ANN;
using DPBloom.Core.Exam.Enums;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace DPBloom.Infrastructure.ANN;

public class OnnxBloomPredictor : IBloomLevelPredictor, IDisposable
{
    private readonly InferenceSession _session;

    public OnnxBloomPredictor()
    {
        _session = new InferenceSession("C:\\Users\\maxim\\OneDrive\\Documents\\University\\4Year\\Diploma\\DPBloom\\DPBloom\\DPBloom.Infrastructure\\ANN\\MlModels\\bloom_model.onnx");
    }

    public async Task<BloomLevel> PredictLevelAsync(string questionText)
    {
        return await Task.Run(() =>
        {
            var onnxInput = TokenizeText(questionText);

            using var results = _session.Run(new List<NamedOnnxValue> { onnxInput });

            var prediction = results.FirstOrDefault(r => r.Name == "output_label")?.AsTensor<long>().First() ?? 0;

            return (BloomLevel)prediction;
        });
    }

    private NamedOnnxValue TokenizeText(string text)
    {
        var inputData = new[] { text };

        var tensor = new DenseTensor<string>(inputData, new[] { 1, 1 });

        return NamedOnnxValue.CreateFromTensor("question_text", tensor);
    }

    private int GetMaxIndex(float[] probabilities)
    {
        return Array.IndexOf(probabilities, probabilities.Max());
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
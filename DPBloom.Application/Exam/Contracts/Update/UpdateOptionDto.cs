
namespace DPBloom.Application.Exam.Contracts.Update;

public class UpdateOptionDto
{
    public Guid? Id { get; set; }
    public string Text { get; set; }
    public bool IsCorrect { get; set; }
}
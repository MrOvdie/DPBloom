using DPBloom.Core.Base;
using TestOfTesting.Models.Enums;
using Type = TestOfTesting.Models.Enums.Type;

namespace DPBloom.Core.Exam;

public class QuestionModel : EntityBase<Guid>
{
    public string Text { get; set; }
    public Guid ExamId { get; set; }
    public int? Position { get; set; }
    public double ScoreWeight { get; set; }
    public Type Type { get; set; }
    public Category Category { get; set; }
    public CheckingType CheckingType { get; set; }
}
using DPBloom.Core.Base;
using DPBloom.Core.Exam.Enums;
using Type = DPBloom.Core.Exam.Enums.Type;

namespace DPBloom.Core.Exam;

public class QuestionModel : EntityBase<Guid>
{
    public string Text { get; set; }
    public Guid ExamId { get; set; }
    public int? Position { get; set; }
    public double ScoreWeight { get; set; }
    public Type Type { get; set; }
    public BloomLevel Level { get; set; }
    public CheckingType CheckingType { get; set; }
}
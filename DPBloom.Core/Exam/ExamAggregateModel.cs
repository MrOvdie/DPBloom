namespace DPBloom.Core.Exam;

public class ExamAggregateModel
{
    public ExamModel Exam { get; set; }
    public List<QuestionModel> Questions { get; set; }
    public List<AnswerOptionModel>? AnswerOptions { get; set; }
}
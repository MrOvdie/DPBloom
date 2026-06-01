namespace DPBloom.Core.Exam;

public class UserExamAttemptAggregateModel
{
    public UserExamAttemptModel Attempt { get; set; } = null!;
    public ExamModel? Exam { get; set; }
    public List<QuestionModel> Questions { get; set; } = new();
    public List<UserQuestionAnswerModel> Answers { get; set; } = new();
}
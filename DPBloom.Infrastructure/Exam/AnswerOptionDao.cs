using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Extensions;

namespace DPBloom.Infrastructure.Exam;

public class AnswerOptionDao : EntityDaoBase<Guid>, ISoftDelete
{
    public string Text { get; set; }
    public Guid QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    
    public QuestionDao Question { get; set; }
}
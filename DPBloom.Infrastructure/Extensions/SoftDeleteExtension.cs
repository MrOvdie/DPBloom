using DPBloom.Infrastructure.Exam;

namespace DPBloom.Infrastructure.Extensions;

public static class SoftDeleteExtension
{
    public static void Undo(this ISoftDelete entity)
    {
        entity.IsDeleted = false;
        entity.DeletedOn = null;
    }

    public static void Delete(this ISoftDelete entity)
    {
        entity.IsDeleted = true;
        entity.DeletedOn = DateTime.UtcNow;
    }
    
    public static void DeleteExamAggregate(this ExamDao exam)
    {
        exam.Delete();

        if (exam.Questions is not null)
        {
            foreach (var question in exam.Questions)
            {
                question.Delete();

                if (question.Options is not null)
                {
                    foreach (var option in question.Options)
                    {
                        option.Delete();
                    }
                }
            }
        }
    }
    
    public static void UndoAggregate(this ExamDao exam)
    {
        exam.Undo();

        foreach (var question in exam.Questions)
        {
            question.Undo();

            foreach (var option in question.Options)
            {
                option.Undo();
            }
        }
    }
}
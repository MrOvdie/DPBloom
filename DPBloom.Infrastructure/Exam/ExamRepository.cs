using AutoMapper;
using DPBloom.Application.Exam;
using DPBloom.Core.Exam;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;
using DPBloom.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Exam;

public class ExamRepository : RepositoryBase<ExamModel, ExamDao, ApplicationDbContext>, IExamRepository
{
    public ExamRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
    }

    public async Task<IReadOnlyList<ExamModel>> GetByCourseAsync(Guid courseId)
    {
        var examsDao = await DbContext.Exams
            .Where(e => e.CourseId.Equals(courseId)).ToListAsync();

        return Mapper.Map<List<ExamModel>>(examsDao);
    }

    /*public async Task<ExamAggregateModel?> GetWithQuestionsAsync(Guid examId)
    {
        var examDao = await DbContext.Exams
            .Include(e => e.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(e => e.Id.Equals(examId));

        return Mapper.Map<ExamAggregateModel>(examDao);
    }*/

    public async Task<ExamAggregateModel?> GetWithQuestionsAsync(Guid examId)
    {
        var examDao = await DbContext.Exams
            .Include(e => e.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(e => e.Id == examId);

        if (examDao is null) return null;

        var aggregate = new ExamAggregateModel
        {
            Exam = Mapper.Map<ExamModel>(examDao),
            Questions = Mapper.Map<List<QuestionModel>>(examDao.Questions ?? new List<QuestionDao>()),

            AnswerOptions = examDao.Questions is not null
                ? Mapper.Map<List<AnswerOptionModel>>(
                    examDao.Questions.SelectMany(q => q.Options ?? new List<AnswerOptionDao>()))
                : []
        };

        return aggregate;
    }

    public async Task<IReadOnlyList<UserQuestionAnswerModel>> GetUserAnswersForQuestionAsync(Guid attemptId,
        Guid questionId)
    {
        var answers = await DbContext.UserAnswers
            .Where(ao => ao.AttemptId.Equals(attemptId) && ao.QuestionId.Equals(questionId))
            .ToListAsync();

        return Mapper.Map<List<UserQuestionAnswerModel>>(answers);
    }

    public async Task<ExamAggregateModel> AddExamWithDetailsAsync(ExamAggregateModel model)
    {
        var examDao = MapExamAggregateToDao(model);

        DbContext.Exams.Add(examDao);
        await DbContext.SaveChangesAsync();

        return MapDaoToExamAggregate(examDao);
    }

    public async Task<ExamAggregateModel> UpdateExamWithDetailsAsync(ExamAggregateModel model)
    {
        var existingExamDao = await DbContext.Exams
            .Include(e => e.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(e => e.Id == model.Exam.Id);

        if (existingExamDao is null)
            throw new KeyNotFoundException($"Exam with ID {model.Exam.Id} not found.");

        DbContext.Entry(existingExamDao).CurrentValues.SetValues(model.Exam);

        var incomingQuestionIds = model.Questions.Select(q => q.Id).ToList();
        var incomingOptionIds = model.AnswerOptions.Select(o => o.Id).ToList();

        var optionsToRemove = existingExamDao.Questions
            .SelectMany(q => q.Options)
            .Where(o => !incomingOptionIds.Contains(o.Id))
            .ToList();
        DbContext.RemoveRange(optionsToRemove);

        var questionsToRemove = existingExamDao.Questions
            .Where(q => !incomingQuestionIds.Contains(q.Id))
            .ToList();
        DbContext.RemoveRange(questionsToRemove);

        foreach (var qModel in model.Questions)
        {
            var existingQuestionDao = existingExamDao.Questions.FirstOrDefault(q => q.Id == qModel.Id);

            if (existingQuestionDao is not null)
            {
                DbContext.Entry(existingQuestionDao).CurrentValues.SetValues(qModel);
            }
            else
            {
                existingQuestionDao = Mapper.Map<QuestionDao>(qModel);
                existingQuestionDao.Options = new List<AnswerOptionDao>();

                DbContext.Entry(existingQuestionDao).State = EntityState.Added;
                existingExamDao.Questions.Add(existingQuestionDao);
            }

            var optionsForQuestion = model.AnswerOptions.Where(o => o.QuestionId == qModel.Id).ToList();

            foreach (var oModel in optionsForQuestion)
            {
                var existingOptionDao = existingQuestionDao.Options.FirstOrDefault(o => o.Id == oModel.Id);

                if (existingOptionDao != null)
                {
                    DbContext.Entry(existingOptionDao).CurrentValues.SetValues(oModel);
                }
                else
                {
                    var newOptionDao = Mapper.Map<AnswerOptionDao>(oModel);

                    DbContext.Entry(newOptionDao).State = EntityState.Added;
                    existingQuestionDao.Options.Add(newOptionDao);
                }
            }
        }

        await DbContext.SaveChangesAsync();

        return MapDaoToExamAggregate(existingExamDao);
    }

    public async Task DeleteExamWithDetailsAsync(ExamAggregateModel model)
    {
        var examDao = MapExamAggregateToDao(model);

        DbContext.ChangeTracker.Clear();

        examDao.DeleteExamAggregate();

        DbContext.Exams.Update(examDao);
        await DbContext.SaveChangesAsync();
    }

    public async Task RestoreExamWithDetailsAsync(ExamAggregateModel model)
    {
        var examDao = MapExamAggregateToDao(model);
        examDao.UndoAggregate();
        await DbContext.SaveChangesAsync();
    }


    private ExamDao MapExamAggregateToDao(ExamAggregateModel model)
    {
        var examDao = Mapper.Map<ExamDao>(model.Exam);

        var answersByQuestionId = model.AnswerOptions
            .GroupBy(ao => ao.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        examDao.Questions = new List<QuestionDao>();
        foreach (var qModel in model.Questions)
        {
            var questionDao = Mapper.Map<QuestionDao>(qModel);

            if (answersByQuestionId.TryGetValue(qModel.Id, out var questionOptions))
            {
                questionDao.Options = Mapper.Map<ICollection<AnswerOptionDao>>(questionOptions);
            }

            examDao.Questions.Add(questionDao);
        }

        return examDao;
    }

    private ExamAggregateModel MapDaoToExamAggregate(ExamDao examDao)
    {
        var examAggregate = new ExamAggregateModel
        {
            Exam = Mapper.Map<ExamModel>(examDao),
            Questions = [],
            AnswerOptions = []
        };

        foreach (var questionDao in examDao.Questions)
        {
            var questionModel = Mapper.Map<QuestionModel>(questionDao);
            examAggregate.Questions.Add(questionModel);

            var answerOptions = Mapper.Map<List<AnswerOptionModel>>(questionDao.Options);
            if (answerOptions is not null && answerOptions.Count != 0)
                examAggregate.AnswerOptions.AddRange(answerOptions);
        }

        return examAggregate;
    }

    public async Task<Guid?> GetCourseIdByExamIdAsync(Guid examId)
    {
        var courseId = await DbContext.Exams.Where(e => e.Id == examId).Select(e => e.CourseId).FirstOrDefaultAsync();

        return courseId;
    }

    public async Task<bool> IsExamAuthorAsync(Guid examId, Guid userId)
    {
        return await DbContext.Exams
            .AnyAsync(c => c.Id == examId && c.AuthorId == userId);
    }
}
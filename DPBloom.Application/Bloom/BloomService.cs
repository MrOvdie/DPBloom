using AutoMapper;
using DPBloom.Application.Attempt;
using DPBloom.Application.Auth;
using DPBloom.Application.Bloom.Contracts;
using DPBloom.Application.Exam;
using DPBloom.Application.Lecture;
using DPBloom.Core.Bloom;
using DPBloom.Core.Exam;
using DPBloom.Core.Exam.Enums;
using DPBloom.Core.Lecture;
using UUIDNext;

namespace DPBloom.Application.Bloom;

public class BloomService : IBloomService
{
    private readonly IBloomRepository _bloomRepository;
    private readonly IRecommendationTemplateRepository _recommendationTemplateRepository;
    private readonly IAttemptResultRepository _attemptResultRepository;
    private readonly IExamRepository _examRepository;
    private readonly ILectureRepository _lectureRepository;
    private readonly IMapper _mapper;

    private const double ContinueThreshold = 60.0;

    public BloomService(
        IBloomRepository bloomRepository,
        IAttemptResultRepository attemptResultRepository,
        IExamRepository examRepository,
        ILectureRepository lectureRepository,
        IMapper mapper, IRecommendationTemplateRepository recommendationTemplateRepository)
    {
        _bloomRepository = bloomRepository;
        _attemptResultRepository = attemptResultRepository;
        _examRepository = examRepository;
        _lectureRepository = lectureRepository;
        _mapper = mapper;
        _recommendationTemplateRepository = recommendationTemplateRepository;
    }

    public async Task<BloomAnalysisDto> AnalyzeAndSaveAttemptAsync(Guid attemptResultId)
    {
        var attemptResult = await _attemptResultRepository.GetAttemptResultByIdAsync(attemptResultId);
        if (attemptResult == null)
            throw new KeyNotFoundException("Attempt result not found.");

        var examAggregate = await _examRepository.GetWithQuestionsAsync(attemptResult.ExamId);
        if (examAggregate == null)
            throw new KeyNotFoundException("Exam not found.");

        var questionsDict = examAggregate.Questions.ToDictionary(q => q.Id, q => q);

        var actualResultsDict = attemptResult.Details
            .Where(d => questionsDict.ContainsKey(d.QuestionId))
            .GroupBy(d => questionsDict[d.QuestionId].Level)
            .Select(group =>
            {
                var maxPossibleScore = group.Sum(d => questionsDict[d.QuestionId].ScoreWeight);
                var userScore = group.Sum(d => d.Score);

                var scorePercentage = maxPossibleScore > 0 ? (userScore / maxPossibleScore) * 100 : 0;

                return new BloomLevelPerformance
                {
                    Level = group.Key,
                    TotalQuestions = group.Count(),
                    CorrectAnswers = group.Count(d => d.IsCorrect),
                    ScorePercentage = scorePercentage,
                    IsWeakPoint = scorePercentage < ContinueThreshold
                };
            }).ToDictionary(x => x.Level, x => x);

        var levelPerformanceList = Enum.GetValues<BloomLevel>().Select(level =>
        {
            if (actualResultsDict.TryGetValue(level, out var performance))
            {
                return performance;
            }

            return new BloomLevelPerformance
            {
                Level = level,
                TotalQuestions = 0,
                CorrectAnswers = 0,
                ScorePercentage = 0,
                IsWeakPoint = false
            };
        }).ToList();

        var weakLevels = levelPerformanceList
            .Where(pl => pl.IsWeakPoint)
            .Select(pl => pl.Level)
            .ToList();

        var recommendations = new List<RecommendedMaterial>();

        if (weakLevels.Count != 0)
        {
            var textTemplates = await _recommendationTemplateRepository.GetTemplatesByLevelsAsync(weakLevels);

            IReadOnlyList<LectureModel> lectures = new List<LectureModel>();
            if (examAggregate.Exam.TopicId.HasValue)
            {
                lectures = await _lectureRepository.GetByTopicAsync(examAggregate.Exam.TopicId.Value);
            }

            foreach (var weakLevel in weakLevels)
            {
                var template = textTemplates.FirstOrDefault(t => t.TargetLevel == weakLevel);
                var adviceText = template != null ? template.AdviceText : "Recommended to read more about this topic.";

                var matchedLecture = lectures.FirstOrDefault(l => l.TargetBloomLevel == weakLevel);

                recommendations.Add(new RecommendedMaterial
                {
                    RelevantBloomLevel = weakLevel,
                    AdviceText = adviceText,
                    MaterialId = matchedLecture?.Id,
                    Title = matchedLecture?.Title
                });
            }
        }

        var bloomAnalysis = new BloomAnalysisModel
        {
            Id = Uuid.NewDatabaseFriendly(Database.SqlServer),
            AttemptResultId = attemptResultId,
            UserId = attemptResult.UserId,
            PerformanceByLevel = levelPerformanceList,
            Recommendations = recommendations,
            OverallFeedback = "",
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow
        };

        var savedAnalysis = await _bloomRepository.AddAnalysisAsync(bloomAnalysis);

        return _mapper.Map<BloomAnalysisDto>(savedAnalysis);
    }

    public async Task<BloomAnalysisDto?> GetAnalysisByAttemptResultIdAsync(Guid attemptResultId)
    {
        var analysis = await _bloomRepository.GetByAttemptResultIdAsync(attemptResultId);

        return _mapper.Map<BloomAnalysisDto>(analysis);
    }

    public async Task<IReadOnlyList<BloomAnalysisDto?>> GetAnalysisByAttemptResultIdsAsync(List<Guid> attemptResultIds)
    {
        var analysis = await _bloomRepository.GetAsync(bam => attemptResultIds.Contains(bam.AttemptResultId));

        return _mapper.Map<List<BloomAnalysisDto>>(analysis);
    }

    public async Task<CourseBloomAnalysisDto> AnalyzeUserCourseAsync(Guid userId, Guid courseId)
    {
        var courseExams = await _examRepository.GetByCourseAsync(courseId);
        var courseExamsIds = courseExams.Select(e => e.Id);

        var userAttemptsResults = await _attemptResultRepository.GetAllAttemptsResultsByUserAsync(userId);
        var courseAttmeptsIds = userAttemptsResults
            .Where(uar => courseExamsIds.Contains(uar.ExamId))
            .Select(uar => uar.Id).ToList();

        if (courseAttmeptsIds.Count == 0)
        {
            return new CourseBloomAnalysisDto { CourseId = courseId, UserId = userId };
        }

        var existedAnalysis = await _bloomRepository.GetByAttemptResultIdsAsync(courseAttmeptsIds);

        var performanceByLevel = existedAnalysis.SelectMany(ba => ba.PerformanceByLevel)
            .GroupBy(pl => pl.Level)
            .Select(group =>
            {
                var averagePercentage = Math.Round(group.Average(p => p.ScorePercentage), 2);

                return new BloomLevelPerformanceDto
                {
                    Level = group.Key,
                    ScorePercentage = averagePercentage,
                    IsWeakPoint = averagePercentage < ContinueThreshold
                };
            }).ToList();

        var recommendations = new List<RecommendedMaterialDto>();
        //TODO: add recommendations logic

        return new CourseBloomAnalysisDto()
        {
            CourseId = courseId,
            UserId = userId,
            AvaragePerformanceByLevel = performanceByLevel,
            CourseRecommendations = recommendations
        };
    }

    public async Task<UserBloomProfileDto> GetUserOverallProfileAsync(Guid userId)
    {
        var userAttemptsResults = await _attemptResultRepository.GetAllAttemptsResultsByUserAsync(userId);

        if (!userAttemptsResults.Any())
        {
            return new UserBloomProfileDto { UserId = userId, TotalExamsAttempts = 0 };
        }

        var examIds = userAttemptsResults.Select(a => a.ExamId).Distinct().ToList();
        var exams = new List<ExamAggregateModel>();

        foreach (var id in examIds)
        {
            var exam = await _examRepository.GetWithQuestionsAsync(id);
            if (exam is not null)
            {
                exams.Add(exam);
            }
        }

        var questionDict = exams.SelectMany(e => e.Questions).ToDictionary(q => q.Id, q => q);

        var actualPerformanceDict = userAttemptsResults
            .SelectMany(a => a.Details)
            .Where(d => questionDict.ContainsKey(d.QuestionId))
            .GroupBy(d => questionDict[d.QuestionId].Level)
            .Select(group =>
            {
                var maxPossibleScore = group.Sum(d => questionDict[d.QuestionId].ScoreWeight);
                var userScore = group.Sum(d => d.Score);

                return new BloomLevelPerformanceDto
                {
                    Level = group.Key,
                    ScorePercentage = maxPossibleScore > 0
                        ? Math.Round(((double)userScore / maxPossibleScore) * 100, 2)
                        : 0
                };
            }).ToDictionary(p => p.Level, p => p);

        var globalPerformance = Enum.GetValues<BloomLevel>().Select(level =>
        {
            if (actualPerformanceDict.TryGetValue(level, out var existingPerformance))
            {
                return existingPerformance;
            }

            return new BloomLevelPerformanceDto
            {
                Level = level,
                ScorePercentage = 0
            };
        }).ToList();

        return new UserBloomProfileDto
        {
            UserId = userId,
            TotalExamsAttempts = examIds.Count,
            GlobalPerformanceByLevel = globalPerformance
        };
    }

    public async Task<ExamGroupAnalysisDto> GetExamAnalysisAsync(Guid examId)
    {
        var examAggregate = await _examRepository.GetWithQuestionsAsync(examId);
        if (examAggregate is null)
        {
            throw new KeyNotFoundException("Exam not found");
        }

        //var questionDict = examAggregate.Questions.ToDictionary(q => q.Id, q => q);

        var allAttemptsForExam = await _attemptResultRepository.GetAllAttemptsResultsByExamAsync(examId);

        var categoriesInExam = examAggregate.Questions.Select(q => q.Level).Distinct();

        var groupPerformance = (from category in categoriesInExam
            let questionsOfCategory = examAggregate.Questions.Where(q => q.Level == category).ToList()
            let questionIds = questionsOfCategory.Select(q => q.Id).ToHashSet()
            let maxScoreForCategory = questionsOfCategory.Sum(q => q.ScoreWeight)
            let attemptPercentages = allAttemptsForExam.Select(attempt =>
            {
                var studentScore = attempt.Details.Where(d => questionIds.Contains(d.QuestionId))
                    .Sum(d => d.Score);
                return maxScoreForCategory > 0 ? (studentScore / maxScoreForCategory) * 100 : 0;
            }).ToList()
            where attemptPercentages.Any()
            let averageScore = attemptPercentages.Average()
            let passRate = attemptPercentages.Count(p => p >= ContinueThreshold) / (double)attemptPercentages.Count *
                           100
            select new BloomLevelGroupPerformanceDto
            {
                Level = category,
                AverageScorePercentage = Math.Round(averageScore, 2),
                PassRatePercentage = Math.Round(passRate, 2),
                IsGroupWeakPoint = averageScore < ContinueThreshold
            }).ToList();

        return new ExamGroupAnalysisDto
        {
            ExamId = examId,
            TotalStudentsParticipated = allAttemptsForExam.Select(a => a.UserId).Distinct().Count(),
            GroupPerformanceByLevel = groupPerformance
            //TODO: add public List<Guid> ProblematicTopicIds { get; set; }  
        };
    }
}
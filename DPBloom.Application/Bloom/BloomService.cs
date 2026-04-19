using AutoMapper;
using DPBloom.Application.Bloom.Contracts;
using DPBloom.Application.Exam;
using DPBloom.Application.Lecture;
using DPBloom.Core.Bloom;
using DPBloom.Core.Exam;
using DPBloom.Core.Lecture;

namespace DPBloom.Application.Bloom;

public class BloomService : IBloomService
{
    private readonly IBloomRepository _bloomRepository;
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
        IMapper mapper)
    {
        _bloomRepository = bloomRepository;
        _attemptResultRepository = attemptResultRepository;
        _examRepository = examRepository;
        _lectureRepository = lectureRepository;
        _mapper = mapper;
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

        var groupedResults = attemptResult.Details
            .Where(d => questionsDict.ContainsKey(d.QuestionId))
            .GroupBy(d => questionsDict[d.QuestionId].Level);

        var levelPerformanceList = (from @group in groupedResults
            let level = @group.Key
            let maxPossibleScore = @group.Sum(d => questionsDict[d.QuestionId].ScoreWeight)
            let userScore = @group.Sum(d => d.Score)
            let scorePercentage = maxPossibleScore > 0 ? (maxPossibleScore / userScore) * 100 : 0

            select new BloomLevelPerformance
            {
                Level = level,
                TotalQuestions = @group.Count(),
                CorrectAnswers = @group.Count(d => d.IsCorrect),
                ScorePercentage = scorePercentage,
                IsWeakPoint = scorePercentage < ContinueThreshold
            }).ToList();

        var weakLevels = levelPerformanceList
            .Where(pl => pl.IsWeakPoint)
            .Select(pl => pl.Level).ToList(); //TODO: prepare for adding topics to tests for recommendations

        var recommendations = new List<RecommendedMaterial>();

        if (weakLevels.Count != 0)
        {
            foreach (var weakLevel in weakLevels)
            {
                IReadOnlyList<LectureModel> recommendedLectures = [];

                if (examAggregate.Exam.TopicId is not null)
                {
                    var topic = examAggregate.Exam.TopicId.Value;
                    recommendedLectures = await _lectureRepository.GetByTopicAsync(topic);
                    //TODO: add bloom's category to lecture model
                }

                recommendations.AddRange(recommendedLectures
                    .Select(lecture => new RecommendedMaterial
                    {
                        MaterialId = lecture.Id,
                        Title = lecture.Title,
                        RelevantBloomLevel = weakLevel
                    }));
            }
        }

        var bloomAnalysis = new BloomAnalysisModel
            {
                Id = Guid.NewGuid(),
                AttemptResultId = attemptResultId,
                UserId = Guid.Empty, //TODO: add user id to attempt result after competing it
                PerformanceByLevel = levelPerformanceList,
                Recommendations = recommendations,
                OverallFeedback = ""/*GenerateFeedback(levelPerformanceList)*/, //TODO: maybe add feedback from teacher or automated one
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
            return new CourseBloomAnalysisDto {CourseId = courseId, UserId = userId};
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
        var exams = new List<ExamAggregateModel>(); //TODO: mb change for more relevant exam model
        
        foreach (var id in examIds)
        {
            var exam = await _examRepository.GetWithQuestionsAsync(id);
            if (exam is not null)
            {
                exams.Add(exam);
            }
        }

        var questionDict = exams.SelectMany(e => e.Questions).ToDictionary(q => q.Id, q => q);

        var globalPerformance = userAttemptsResults
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
                    ScorePercentage = maxPossibleScore > 0 ? Math.Round((userScore / maxPossibleScore) * 100, 2) : 0,
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
            let passRate = attemptPercentages.Count(p => p >= ContinueThreshold) / (double)attemptPercentages.Count * 100
            select new BloomLevelGroupPerformanceDto { 
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
using AutoMapper;
using DPBloom.Application.Attempt;
using DPBloom.Application.Auth;
using DPBloom.Application.Bloom.Contracts;
using DPBloom.Application.Course;
using DPBloom.Application.Enrollment;
using DPBloom.Application.Exam;
using DPBloom.Application.User.Contracts;
using DPBloom.Core.Exam;
using DPBloom.Core.Exam.Enums;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Application.User;

public class UserService : IUserService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IExamRepository _examRepository;
    private readonly IAttemptResultRepository _attemptResultRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICourseService _courseService;


    public UserService(ICurrentUserService currentUserService, IExamRepository examRepository,
        IAttemptResultRepository attemptResultRepository, ICourseService coureseService,
        IEnrollmentRepository enrollmentRepository, IUserRepository userRepository, IMapper mapper)
    {
        _currentUserService = currentUserService;
        _examRepository = examRepository;
        _attemptResultRepository = attemptResultRepository;
        _courseService = coureseService;
        _enrollmentRepository = enrollmentRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
    {
        var userModel = await _userRepository.GetUserByIdAsync(userId);

        if (userModel is null) return null;

        var roles = (await _userRepository.GetUserClaimsAsync(userId))
            .Select(c => c.Value)
            .ToList();

        var profileDto = _mapper.Map<UserProfileDto>(userModel);

        profileDto.Roles = roles;

        return profileDto;
    }

    public async Task<IReadOnlyList<UserBaseInformationDto>> GetUserBaseInformationByIdsAsync(List<Guid> userIds)
    {
        if (userIds is null || userIds.Count == 0)
            return new List<UserBaseInformationDto>();

        var uniqueUserIds = userIds.Distinct().ToList();

        var users = await _userRepository.GetUsersByIdsAsync(uniqueUserIds);

        return _mapper.Map<List<UserBaseInformationDto>>(users);
    }
    
    public async Task<UserBaseInformationDto> GetUserBaseInformationByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);

        return _mapper.Map<UserBaseInformationDto>(user);
    }

    public async Task<GlobalUserDashboardDto> GetGlobalUserDashboardStatisticsAsync(Guid userId)
    {
        _ = EnsureUserHasAccessToFullProfileStatisticAsync(userId);

        var dashboardData = new GlobalUserDashboardDto();

        var enrolledCourseIds = await _enrollmentRepository.GetAllIdsByUserIdAsync(userId);

        if (enrolledCourseIds.Any())
        {
            double totalScoreSum = 0;
            double totalCompletionSum = 0;

            foreach (var courseId in enrolledCourseIds)
            {
                totalScoreSum += await _courseService.GetUserCourseScoreAsync(courseId, userId, skipAccessCheck: true);
                totalCompletionSum +=
                    await _courseService.GetCourseExamsProgressPercentAsync(courseId, userId, skipAccessCheck: true);
            }

            dashboardData.AverageCourseScore = Math.Round(totalScoreSum / enrolledCourseIds.Count, 2);
            dashboardData.AverageExamCompletion = Math.Round(totalCompletionSum / enrolledCourseIds.Count, 2);
        }

        var allUserAttemptsResults = await _attemptResultRepository.GetAllAttemptsResultsByUserAsync(userId);

        if (allUserAttemptsResults is null || !allUserAttemptsResults.Any())
        {
            dashboardData.BloomPerformance = Enum.GetValues<BloomLevel>()
                .Select(level => new BloomLevelPerformanceDto { Level = level, ScorePercentage = 0 })
                .ToList();

            return dashboardData;
        }

        var examIds = allUserAttemptsResults.Select(a => a.ExamId).Distinct().ToList();
        var allQuestionsDict = new Dictionary<Guid, QuestionModel>();

        foreach (var examId in examIds)
        {
            var exam = await _examRepository.GetWithQuestionsAsync(examId);
            if (exam != null)
            {
                foreach (var question in exam.Questions)
                {
                    allQuestionsDict.TryAdd(question.Id, question);
                }
            }
        }

        var actualPerformanceDict = allUserAttemptsResults
            .SelectMany(a => a.Details)
            .Where(d => allQuestionsDict.ContainsKey(d.QuestionId))
            .GroupBy(d => allQuestionsDict[d.QuestionId].Level)
            .Select(group =>
            {
                var maxPossibleScore = group.Sum(d => allQuestionsDict[d.QuestionId].ScoreWeight);
                var userScore = group.Sum(d => d.Score);

                return new BloomLevelPerformanceDto
                {
                    Level = group.Key,
                    ScorePercentage = maxPossibleScore > 0 ? Math.Round((userScore / maxPossibleScore) * 100, 2) : 0
                };
            }).ToDictionary(p => p.Level, p => p);

        dashboardData.BloomPerformance = Enum.GetValues<BloomLevel>().Select(level =>
            actualPerformanceDict.TryGetValue(level, out var existingPerformance)
                ? existingPerformance
                : new BloomLevelPerformanceDto { Level = level, ScorePercentage = 0 }
        ).ToList();

        return dashboardData;
    }

    private Task EnsureUserHasAccessToFullProfileStatisticAsync(Guid userId)
    {
        if (_currentUserService.IsAdmin())
            return Task.CompletedTask;

        var currentUserId = _currentUserService.GetUserId();

        if (userId != currentUserId)
            throw new UnauthorizedAccessException("Only the student, or admins can view this statistic.");
        return Task.CompletedTask;
    }
}
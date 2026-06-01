using DPBloom.Application.User.Contracts;

namespace DPBloom.Application.User;

public interface IUserService
{
    Task<GlobalUserDashboardDto> GetGlobalUserDashboardStatisticsAsync(Guid userId);
    Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
    Task<IReadOnlyList<UserBaseInformationDto>> GetUserBaseInformationByIdsAsync(List<Guid> userIds);
    Task<UserBaseInformationDto> GetUserBaseInformationByIdAsync(Guid userId);
}
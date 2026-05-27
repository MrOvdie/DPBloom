using DPBloom.Application.User.Contracts;

namespace DPBloom.Application.User;

public interface IUserService
{
    Task<GlobalUserDashboardDto> GetGlobalUserDashboardStatisticsAsync(Guid userId);
}
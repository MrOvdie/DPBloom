using DPBloom.Application.User;

namespace DPBloom.Application.Auth;

public interface ICurrentUserService
{
    Guid GetUserId();
    bool IsAdmin();
}
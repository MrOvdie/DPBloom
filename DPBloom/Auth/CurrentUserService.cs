using System.Security.Claims;
using AutoMapper;
using DPBloom.Application.Auth;
using DPBloom.Application.User;

namespace DPBloom.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository, IMapper mapper)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public Guid GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(userId) ? Guid.Empty : Guid.Parse(userId);
    }

    public bool IsAdmin()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null) return false;
        
        return user?.IsInRole("Admin") ?? false;
    }
    
    /*public bool IsAdmin()
{
    var userClaims = _httpContextAccessor.HttpContext?.User?.Claims;
    
    return userClaims?.Any(c => 
        c.QuestionType == "user_role" && // Обов'язково перевіряємо ТИП
        c.Value == "Admin"       // І тільки потім ЗНАЧЕННЯ
    ) ?? false;
}*/
}
using System.Security.Claims;
using DPBloom.Application.Auth;
using DPBloom.Core.User;
using Microsoft.AspNetCore.Identity;

namespace DPBloom.Infrastructure.User;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserModel?> ValidateUserAsync(string email, string password)
    {
        var appUser = await _userManager.FindByEmailAsync(email);
        
        // 2. Якщо користувача немає або пароль невірний - повертаємо null
        if (appUser is null || !await _userManager.CheckPasswordAsync(appUser, password))
        {
            return null;
        }

        // 3. Мапимо (перекладаємо) базу даних у чисту бізнес-модель
        return new UserModel
        {
            Id = appUser.Id,
            Email = appUser.Email ?? string.Empty,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName
        };
    }

    public async Task<IReadOnlyList<Claim>> GetUserClaimsAsync(Guid userId)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString());
        
        if (appUser is null)
        {
            return new List<Claim>();
        }

        var claims = await _userManager.GetClaimsAsync(appUser);

        return claims.ToList();
    }


    public async Task<bool> CreateUserAsync(UserModel user, string password, string initialRole)
    {
        var appUser = new ApplicationUser
        {
            Id = user.Id,
            UserName = user.Email,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
        
        var result = await _userManager.CreateAsync(appUser, password); //TODO: check saving process

        if (result.Succeeded)
        {
            await _userManager.AddClaimAsync(appUser, new Claim(ClaimTypes.Role, initialRole)); //TODO: maybe add some more claims
        }

        return result.Succeeded;
    
    }

    public async Task<UserModel?> GetByIdAsync(Guid userId)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString());
    
        if (appUser is null) return null;

        return new UserModel //TODO: check if this layer is allowed to map to domain model
        {
            Id = appUser.Id,
            Email = appUser.Email ?? string.Empty,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName
        };
    }
}
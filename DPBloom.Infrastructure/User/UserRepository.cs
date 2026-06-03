using System.Security.Claims;
using AutoMapper;
using DPBloom.Application.Auth;
using DPBloom.Core.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.User;

public class UserRepository : IUserRepository
{
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<UserModel?> ValidateUserAsync(string loginDetails, string password)
    {
        var appUser = await _userManager.FindByEmailAsync(loginDetails)
                      ?? await _userManager.FindByNameAsync(loginDetails);

        if (appUser is null || !await _userManager.CheckPasswordAsync(appUser, password))
        {
            return null;
        }

        var userClaims = await _userManager.GetClaimsAsync(appUser);

        var roles = userClaims
            .Where(c => c.Type is ClaimTypes.Role or "role")
            .Select(c => c.Value)
            .ToList();

        var userModel = _mapper.Map<UserModel>(appUser);

        userModel.RoleClaims = roles;

        return userModel;
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
        var appUser = _mapper.Map<ApplicationUser>(user);

        var result = await _userManager.CreateAsync(appUser, password); //TODO: check saving process

        if (result.Succeeded)
        {
            await _userManager.AddClaimAsync(appUser,
                new Claim(ClaimTypes.Role, initialRole)); //TODO: maybe add some more claims
        }

        return result.Succeeded;
    }

    public async Task<bool> UpdateUserAsync(UserModel userModel)
    {
        var appUser = await _userManager.FindByIdAsync(userModel.Id.ToString());
        if (appUser is null) return false;

        var isFirstLogin = false;

        if (appUser.FirstLogin == default)
        {
            appUser.FirstLogin = DateTime.UtcNow;
            isFirstLogin = true; //TODO: after maybe add event on this
        }

        appUser.LastLogin = DateTime.UtcNow;

        await _userManager.UpdateAsync(appUser);

        return true;
    }

    public async Task<UserModel?> GetUserByIdAsync(Guid userId)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString());

        return _mapper.Map<UserModel>(appUser);
    }

    public async Task<IReadOnlyList<UserModel?>> GetUsersByIdsAsync(List<Guid> userIds)
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();
        
        return _mapper.Map<List<UserModel>>(users);
    }

    public async Task<UserModel?> GetUserByNameAsync(string userName)
    {
        var appUser = await _userManager.FindByNameAsync(userName);

        return _mapper.Map<UserModel>(appUser);
    }

    public async Task<IReadOnlyList<UserModel?>> GetUsersByGroupAsync(string group)
    {
        var appUsers = await _userManager.Users
            .Where(u => u.Group == group)
            .ToListAsync();
    
        return _mapper.Map<List<UserModel?>>(appUsers);
    }

    public async Task<string> GetUserAccessTokens(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var token = await _userManager.GetAuthenticationTokenAsync(user, "DPBloomApp", "RefreshToken");
        
        return token;
    }

    public async Task RemoveRefreshTokenAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            await _userManager.RemoveAuthenticationTokenAsync(user, "DPBloom", "RefreshToken");
        }
    }
}
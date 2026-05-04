// DPBloom.Infrastructure/Auth/AuthService.cs

using System.Security.Claims;
using System.Text;
using DPBloom.Core.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace DPBloom.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<bool> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        var newUser = new UserModel
        {
            Id = Guid.NewGuid(),  //TODO: check it
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        return await _userRepository.CreateUserAsync(newUser, password, "Student"); //all have initial student claim
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _userRepository.ValidateUserAsync(email, password);
        
        if (user == null) 
        {
            return null;
        }

        var claims = await _userRepository.GetUserClaimsAsync(user.Id);

        return _jwtTokenGenerator.GenerateToken(user, claims);
    }
}
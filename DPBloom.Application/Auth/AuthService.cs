using System.Security.Claims;
using AutoMapper;
using DPBloom.Application.Auth.Contracts;
using DPBloom.Application.User;
using DPBloom.Core.User;
using FluentValidation;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DPBloom.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IMapper _mapper;
    private readonly IValidator<RegisterUserDto> _registerValidator;
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, IMapper mapper,
        IValidator<RegisterUserDto> registerValidator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _mapper = mapper;
        _registerValidator = registerValidator;
    }

    public async Task<bool> RegisterAsync(RegisterUserDto user)
    {
        var validationResult = await _registerValidator.ValidateAsync(user);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var newUser = _mapper.Map<UserModel>(user);

        newUser.Id = Guid.NewGuid();
        newUser.CreatedOn = newUser.UpdatedOn = DateTime.UtcNow;
        newUser.UserName =
            $"{DateTime.UtcNow.Year}{user.Group.ToLower()}{user.Faculty.ToLower()}{user.FirstName.ToLower().First()}{user.MiddleName.ToLower().First()}{user.LastName.ToLower().First()}";

        return await _userRepository.CreateUserAsync(newUser, user.Password,
            "Student");
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request) //TODO: REWORK THIS CRAP
    {
        var userModel = await _userRepository.ValidateUserAsync(request.LoginDetails, request.Password);

        if (userModel is null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userModel.Email),
            new Claim(ClaimTypes.NameIdentifier, userModel.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in userModel.RoleClaims)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = _jwtTokenGenerator.GenerateToken(userModel, claims);

        return new AuthResponseDto
            { Token = token, FullName = userModel.GetFullName(), AvatarUrl = userModel.AvatarUrl };

        /*var userModel = await _userRepository.ValidateUserAsync(request.LoginDetails, request.Password);

        if (userModel is null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        await _userRepository.UpdateUserAsync(userModel);

        // 1. Створюємо базові клейми
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userModel.Email),
            new Claim(ClaimTypes.NameIdentifier, userModel.Id.ToString())
        };

        // 2. ДОДАЄМО РОЛІ (перебираємо саме Roles з моделі)
        if (userModel.Roles is not null)
        {
            foreach (var roleName in userModel.Roles)
            {
                // Переконайся, що додаєш саме значення рядка (Admin, Student тощо)
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }
        }

        var token = _jwtTokenGenerator.GenerateToken(userModel, claims);

        return new AuthResponseDto
            { Token = token, FullName = userModel.GetFullName(), AvatarUrl = userModel.AvatarUrl };*/
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
    {
        var userModel = await _userRepository.GetByIdAsync(userId);

        if (userModel is null) return null;

        var roles = (await _userRepository.GetUserClaimsAsync(userId))
            .Select(c => c.Value)
            .ToList();

        var profileDto = _mapper.Map<UserProfileDto>(userModel);

        profileDto.Roles = roles;

        return profileDto;
    }
}
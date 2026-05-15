using System.Security.Claims;
using AutoMapper;
using DPBloom.Application.Auth.Contracts;
using DPBloom.Application.User;
using DPBloom.Core.User;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DPBloom.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher<UserModel> _passwordHasher; 
    private readonly IMapper _mapper;
    private readonly IValidator<RegisterUserDto> _registerValidator;
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, IMapper mapper,
        IValidator<RegisterUserDto> registerValidator, IPasswordHasher<UserModel> passwordHasher)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _mapper = mapper;
        _registerValidator = registerValidator;
        _passwordHasher = passwordHasher;
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

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var userModel = await _userRepository.ValidateUserAsync(request.LoginDetails, request.Password);

        if (userModel is null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = _jwtTokenGenerator.GenerateToken(userModel); //TODO: CAN BE PROBLEM HERE -> uncomment old code

        var response = new AuthResponseDto
        { 
            Token = token, 
            FullName = userModel.GetFullName(), 
            AvatarUrl = userModel.AvatarUrl 
        };

        return response;
        
        /*var userModel = await _userRepository.ValidateUserAsync(request.LoginDetails, request.Password);

        if (userModel is null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userModel.UserName),
            new Claim(ClaimTypes.Email, userModel.Email),
            new Claim(ClaimTypes.NameIdentifier, userModel.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in userModel.RoleClaims)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = _jwtTokenGenerator.GenerateToken(userModel, claims);

        return new AuthResponseDto
            { Token = token, FullName = userModel.GetFullName(), AvatarUrl = userModel.AvatarUrl };*/
    }
    
    public async Task<UserProfileDto> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto dto)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
    
        if (user is null)
            throw new KeyNotFoundException($"Can't find user {userId}.");
        
        var updatedUser = _mapper.Map<UserModel>(dto);
        
        await _userRepository.UpdateUserAsync(updatedUser);

        return _mapper.Map<UserProfileDto>(updatedUser);
    }
    
    public async Task ChangeUserPasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user is null)
            throw new KeyNotFoundException($"Can't find user {userId}.");

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);

        if (verificationResult == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
        user.UpdatedOn = DateTime.UtcNow;

        await _userRepository.UpdateUserAsync(user);
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
}
using AutoMapper;
using DPBloom.Application.Auth.Contracts;
using DPBloom.Application.User;
using DPBloom.Core.User;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using UUIDNext;

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

        var group = user.Group?.ToLower() ?? "";
        var faculty = user.Faculty?.ToLower() ?? "";

        var fName = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName.ToLower()[0].ToString() : "";
        var mName = !string.IsNullOrEmpty(user.MiddleName) ? user.MiddleName.ToLower()[0].ToString() : "";
        var lName = !string.IsNullOrEmpty(user.LastName) ? user.LastName.ToLower()[0].ToString() : "";

        newUser.UserName = $"{DateTime.UtcNow.Year}{group}{faculty}{fName}{mName}{lName}";
        
        newUser.Id = Uuid.NewDatabaseFriendly(Database.SqlServer);
        newUser.CreatedOn = newUser.UpdatedOn = DateTime.UtcNow;
        newUser.UserName = newUser.UserName = $"{DateTime.UtcNow.Year}{group}{faculty}{fName}{mName}{lName}";

        return await _userRepository.CreateUserAsync(newUser, user.Password,
            "Student");
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var userModel = await _userRepository.ValidateUserAsync(request.LoginDetails, request.Password);

        if (userModel is null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = _jwtTokenGenerator.GenerateToken(userModel);

        var response = new AuthResponseDto
        { 
            UserId = userModel.Id,
            Token = token, 
            FullName = userModel.GetFullName(), 
            Username = userModel.UserName,
            AvatarUrl = userModel.AvatarUrl
        };

        return response;
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
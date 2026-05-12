using AutoMapper;
using DPBloom.Application.User.Contracts;
using DPBloom.Core.User;
using FluentValidation;

namespace DPBloom.Application.User;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IValidator<CreateEnrollment> _createEnrollmentValidator;
    private readonly IValidator<UpdateEnrollment> _updateEnrollmentValidator;
    private readonly IMapper _mapper;
    
    public EnrollmentService(IEnrollmentRepository enrollmentRepository, IMapper mapper, IValidator<CreateEnrollment> createEnrollmentValidator, IValidator<UpdateEnrollment> updateEnrollmentValidator)
    {
        _enrollmentRepository = enrollmentRepository;
        _createEnrollmentValidator = createEnrollmentValidator;
        _updateEnrollmentValidator = updateEnrollmentValidator;
        _mapper = mapper;
    }
    
    public async Task<IEnumerable<UserEnrollmentDto>> GetAllAsync()
    {
        var enrollments = await _enrollmentRepository.GetAllAsync();
        
        return _mapper.Map<IEnumerable<UserEnrollmentDto>>(enrollments);
    }
    
    public async Task<UserEnrollmentDto> GetByUserAndCourseAsync(Guid userId, Guid courseId)
    {
        var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, courseId);
        
        if (enrollment is null)
            throw new KeyNotFoundException($"Enrollment for user {userId} to the course {courseId} not found");
        
        return _mapper.Map<UserEnrollmentDto>(enrollment);
    }

    public async Task<IEnumerable<UserEnrollmentDto>> GetAllByUserAsync(Guid userId)
    {
        var enrollments = await _enrollmentRepository.GetAllByUserIdAsync(userId);
        
        return _mapper.Map<IEnumerable<UserEnrollmentDto>>(enrollments);
    }
    
    public async Task<IEnumerable<UserEnrollmentDto>> GetAllByCourseAsync(Guid courseId)
    {
        var enrollments = await _enrollmentRepository.GetAllByCourseIdAsync(courseId);
        
        return _mapper.Map<IEnumerable<UserEnrollmentDto>>(enrollments);
    }

    public async Task<bool> CreateAsync(Guid courseId, CreateEnrollment createEnrollment)
    {
        var validationResult = await _createEnrollmentValidator.ValidateAsync(createEnrollment);
        
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        if (await _enrollmentRepository.ExistsAsync(c => c.UserId.Equals(createEnrollment.UserId) 
                                                         && c.CourseId == courseId))
            throw new InvalidOperationException("Student is already enrolled in this course");
        
        var createdEnrollmentModel = _mapper.Map<UserEnrollmentModel>(createEnrollment);
        createdEnrollmentModel.CourseId = courseId;

        var createdEnrollment = await _enrollmentRepository.AddAsync(createdEnrollmentModel);
        
        if (createdEnrollment is null)
            throw new InvalidOperationException("Failed to create enrollment");
            
        return true;
    }
    
    public async Task<UserEnrollmentDto> UpdateAsync(Guid userId, Guid courseId, UpdateEnrollment updateEnrollment)
    {
        var validationResult = await _updateEnrollmentValidator.ValidateAsync(updateEnrollment);
        
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingEnrollment = await GetEntityByIdAsync(userId, courseId);
        
        var updatedExistedEnrollmentModel = _mapper.Map(updateEnrollment, existingEnrollment);
        updatedExistedEnrollmentModel.UpdatedOn = DateTime.UtcNow;
        
        var updatedEnrollment = await _enrollmentRepository.UpdateAsync(updatedExistedEnrollmentModel);
        
        return _mapper.Map<UserEnrollmentDto>(updatedEnrollment);
    }

    public async Task<UserEnrollmentDto> DeleteAsync(Guid enrollmentId)
    {
        var enrollment = _enrollmentRepository.GetByIdAsync(enrollmentId);
        
        await _enrollmentRepository.DeleteAsync(enrollmentId);

        return _mapper.Map<UserEnrollmentDto>(enrollment);
    }
    
    public async Task<UserEnrollmentDto> RestoreUserEnrollmentAsync(Guid enrollmentId)
    {
        var restoredEnrollment = await _enrollmentRepository.RestoreAsync(enrollmentId);
        
        return _mapper.Map<UserEnrollmentDto>(restoredEnrollment);
    }
    
    public async Task<UserEnrollmentModel> GetEntityByIdAsync(Guid userId, Guid courseId)
    {
        var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, courseId);
        if (enrollment is null)
            throw new KeyNotFoundException($"Enrollment for user {userId} to the course {courseId} not found");

        return enrollment;
    }
}
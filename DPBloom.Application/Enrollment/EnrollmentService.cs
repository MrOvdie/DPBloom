using AutoMapper;
using DPBloom.Application.Enrollment.Contracts;
using DPBloom.Core.User;
using FluentValidation;

namespace DPBloom.Application.Enrollment;

public class EnrollmentService : IEnrollmentService
{
    private readonly IValidator<CreateEnrollment> _createEnrollmentValidator;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateEnrollment> _updateEnrollmentValidator;

    public EnrollmentService(IEnrollmentRepository enrollmentRepository, IMapper mapper,
        IValidator<CreateEnrollment> createEnrollmentValidator, IValidator<UpdateEnrollment> updateEnrollmentValidator)
    {
        _enrollmentRepository = enrollmentRepository;
        _createEnrollmentValidator = createEnrollmentValidator;
        _updateEnrollmentValidator = updateEnrollmentValidator;
        _mapper = mapper;
    }

    public async Task<bool> CheckUserEnrollment(Guid userId, Guid courseId)
    {
        var enrollment = await _enrollmentRepository.ExistsAsync(userId, courseId);

        return enrollment;
    }

    public async Task CreateAsync(CreateEnrollment createEnrollment)
    {
        var validationResult = await _createEnrollmentValidator.ValidateAsync(createEnrollment);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingEnrollment =
            await _enrollmentRepository.ExistsAsync(createEnrollment.UserId, createEnrollment.CourseId);

        if (existingEnrollment)
            throw new InvalidOperationException("Student is already enrolled in this course");

        //TODO: check if I accidentally missed smth

        var createdEnrollmentModel = _mapper.Map<UserEnrollmentModel>(createEnrollment);
        createdEnrollmentModel.CourseId = createEnrollment.CourseId;

        var createdEnrollment = await _enrollmentRepository.AddAsync(createdEnrollmentModel);

        if (createdEnrollment is null)
            throw new InvalidOperationException("Failed to create enrollment");
    }

    public async Task<UserEnrollmentDto> UpdateAsync(Guid enrollmentId, UpdateEnrollment updateEnrollment)
    {
        var validationResult = await _updateEnrollmentValidator.ValidateAsync(updateEnrollment);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingEnrollment = await GetEntityByIdAsync(enrollmentId);

        var updatedExistedEnrollmentModel = _mapper.Map(updateEnrollment, existingEnrollment);
        updatedExistedEnrollmentModel.UpdatedOn = DateTime.UtcNow;

        var updatedEnrollment = await _enrollmentRepository.UpdateAsync(updatedExistedEnrollmentModel);

        return _mapper.Map<UserEnrollmentDto>(updatedEnrollment);
    }

    public async Task<UserEnrollmentDto> DeleteUserEnrollmentAsync(Guid enrollmentId)
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

    public async Task<UserEnrollmentModel> GetEntityByIdAsync(Guid enrollmentId)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
        if (enrollment is null)
            throw new KeyNotFoundException($"Enrollment {enrollmentId} not found");

        return enrollment;
    }
}
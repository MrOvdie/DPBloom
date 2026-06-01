using DPBloom.Application.Enrollment.Contracts;
using DPBloom.Core.User;

namespace DPBloom.Application.Enrollment;

public interface IEnrollmentService
{
    public Task CreateAsync(CreateEnrollmentDto createEnrollmentDto);
    public Task<UserEnrollmentDto> UpdateAsync(Guid enrollmentId, UpdateEnrollmentDto updateEnrollmentDto);
    public Task<UserEnrollmentDto> DeleteUserEnrollmentAsync(Guid enrollmentId);
    public Task<UserEnrollmentDto> RestoreUserEnrollmentAsync(Guid enrollmentId);
    public Task<UserEnrollmentModel> GetEntityByIdAsync(Guid enrollmentId);
    Task<bool> CheckUserEnrollment(Guid userId, Guid courseId);
}
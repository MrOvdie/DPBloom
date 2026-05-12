using DPBloom.Application.Enrollment.Contracts;
using DPBloom.Core.User;

namespace DPBloom.Application.Enrollment;

public interface IEnrollmentService
{
    public Task CreateAsync(CreateEnrollment createEnrollment);
    public Task<UserEnrollmentDto> UpdateAsync(Guid enrollmentId, UpdateEnrollment updateEnrollment);
    public Task<UserEnrollmentDto> DeleteUserEnrollmentAsync(Guid enrollmentId);
    public Task<UserEnrollmentDto> RestoreUserEnrollmentAsync(Guid enrollmentId);
    public Task<UserEnrollmentModel> GetEntityByIdAsync(Guid enrollmentId);
    Task<bool> CheckUserEnrollment(Guid userId, Guid courseId);
}
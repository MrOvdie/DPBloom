using DPBloom.Application.User.Contracts;
using DPBloom.Core.User;

namespace DPBloom.Application.User;

public interface IEnrollmentService
{
    public Task<IEnumerable<UserEnrollmentDto>> GetAllAsync();
    public Task<UserEnrollmentDto> GetByUserAndCourseAsync(Guid userId, Guid courseId);
    public Task<IEnumerable<UserEnrollmentDto>> GetAllByUserAsync(Guid userId);
    public Task<IEnumerable<UserEnrollmentDto>> GetAllByCourseAsync(Guid courseId);
    public Task<bool> CreateAsync(Guid courseId, CreateEnrollment createEnrollment);
    public Task<UserEnrollmentDto> UpdateAsync(Guid userId, Guid courseId, UpdateEnrollment updateEnrollment);
    public Task<UserEnrollmentDto> DeleteAsync(Guid enrollmentId);
    public Task<UserEnrollmentDto> RestoreUserEnrollmentAsync(Guid enrollmentId);
    public Task<UserEnrollmentModel> GetEntityByIdAsync(Guid userId, Guid courseId);
}
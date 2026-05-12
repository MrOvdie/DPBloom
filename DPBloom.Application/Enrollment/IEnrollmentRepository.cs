using DPBloom.Application.Base;
using DPBloom.Core.User;

namespace DPBloom.Application.Enrollment;

public interface IEnrollmentRepository : IRepository<UserEnrollmentModel>
{
    Task<IReadOnlyList<UserEnrollmentModel>> GetAllByUserIdAsync(Guid userId);
    Task<IReadOnlyList<UserEnrollmentModel>> GetAllByCourseIdAsync(Guid courseId);
    Task<UserEnrollmentModel> GetByUserAndCourseAsync(Guid userId, Guid courseId);
    Task<bool> ExistsAsync(Guid userId, Guid courseId);
}
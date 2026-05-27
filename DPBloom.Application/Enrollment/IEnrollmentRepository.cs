using DPBloom.Application.Base;
using DPBloom.Core.User;

namespace DPBloom.Application.Enrollment;

public interface IEnrollmentRepository : IRepository<UserEnrollmentModel>
{
    Task<IReadOnlyList<UserEnrollmentModel>> GetAllByUserIdAsync(Guid userId);
    Task<IReadOnlyList<Guid>> GetAllIdsByUserIdAsync(Guid userId);
    Task<IReadOnlyList<UserEnrollmentModel>> GetAllByCourseIdAsync(Guid courseId);
    Task<UserEnrollmentModel> GetByUserAndCourseAsync(Guid userId, Guid courseId);
    Task<IReadOnlyList<Guid>> GetEnrolledCourseIdsAsync(Guid userId, List<Guid> courseIds);
    Task<bool> ExistsAsync(Guid userId, Guid courseId);
}
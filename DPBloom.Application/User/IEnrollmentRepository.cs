using DPBloom.Application.Base;
using DPBloom.Core.User;

namespace DPBloom.Application.User;

public interface IEnrollmentRepository : IRepository<UserEnrollmentModel>
{
    Task<IReadOnlyList<UserEnrollmentModel>> GetAllByUserIdAsync(Guid userId);
    Task<IReadOnlyList<UserEnrollmentModel>> GetAllByCourseIdAsync(Guid courseId);
    Task<UserEnrollmentModel> GetByUserAndCourseAsync(Guid userId, Guid courseId);
}
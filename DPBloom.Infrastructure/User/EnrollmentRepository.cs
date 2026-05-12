using AutoMapper;
using DPBloom.Application.User;
using DPBloom.Core.User;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.User;

public class EnrollmentRepository : RepositoryBase<UserEnrollmentModel, UserEnrollmentDao, ApplicationDbContext>,
    IEnrollmentRepository
{
    public EnrollmentRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task<IReadOnlyList<UserEnrollmentModel>> GetAllByUserIdAsync(Guid userId)
    {
        var enrollment = await DbContext.UserEnrollments
            .Where(e => e.UserId.Equals(userId))
            .ToListAsync();

        return Mapper.Map<List<UserEnrollmentModel>>(enrollment);
    }

    public async Task<IReadOnlyList<UserEnrollmentModel>> GetAllByCourseIdAsync(Guid courseId)
    {
        var enrollment = await DbContext.UserEnrollments
            .Where(e => e.CourseId.Equals(courseId))
            .ToListAsync();

        return Mapper.Map<List<UserEnrollmentModel>>(enrollment);
    }

    public async Task<UserEnrollmentModel> GetByUserAndCourseAsync(Guid userId, Guid courseId)
    {
        var enrollment = await DbContext.UserEnrollments
            .FirstOrDefaultAsync(e => e.CourseId.Equals(courseId) && e.UserId.Equals(userId));

        return Mapper.Map<UserEnrollmentModel>(enrollment);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid courseId)
    {
        return await DbContext.UserEnrollments
            .AnyAsync(e => e.CourseId == courseId && e.UserId == userId);
    }
}
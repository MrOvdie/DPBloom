using AutoMapper;
using AutoMapper.QueryableExtensions;
using DPBloom.Application.Course;
using DPBloom.Application.Course.Contracts;
using DPBloom.Core.Course;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Course;

public class CourseRepository : RepositoryBase<CourseModel, CourseDao, ApplicationDbContext>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
    }

    public async Task<CourseAggregateDto?> GetCourseWithContentAsync(Guid courseId)
    {
        return await DbContext.Courses
            .Where(c => c.Id == courseId)
            .ProjectTo<CourseAggregateDto>(Mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }
    
    public async Task<IReadOnlyList<CourseModel>> GetEnrolledCoursesByUserIdAsync(Guid userId)
    {
        var enrolledCourse = await DbContext.UserEnrollments
            .Where(e => e.UserId == userId)
            .Select(e => e.Course) 
            .ToListAsync();
        
        return Mapper.Map<List<CourseModel>>(enrolledCourse);
    }
    
    public async Task<bool> IsCourseAuthorAsync(Guid courseId, Guid userId)
    {
        return await DbContext.Courses
            .AnyAsync(c => c.Id == courseId && c.AuthorId == userId);
    }

    public async Task<Guid> GetTeacherIdByCourseAsync(Guid courseId)
    {
        return await DbContext.Courses
            .Where(c => c.Id == courseId)
            .Select(c => c.AuthorId)
            .FirstOrDefaultAsync();
    }
}
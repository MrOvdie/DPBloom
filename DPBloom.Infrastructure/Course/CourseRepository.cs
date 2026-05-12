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
}
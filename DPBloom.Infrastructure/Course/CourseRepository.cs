using AutoMapper;
using DPBloom.Core.Course;
using DPBloom.Infrastructure.Base;

namespace DPBloom.Infrastructure.Course;

public class CourseRepository : RepositoryBase<CourseModel, CourseDao, ApplicationDbContext>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
    }
}
using AutoMapper;
using DPBloom.Core.Course;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;
using DPBloom.Infrastructure.Exam;

namespace DPBloom.Infrastructure.Course;

public class CourseRepository : RepositoryBase<CourseModel, CourseDao, ApplicationDbContext>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
    }
}
using AutoMapper;
using DPBloom.Core.Exam;
using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;

namespace DPBloom.Infrastructure.Exam;

public class AttemptResultRepository : RepositoryBase<AttemptResultModel, UserExamAttemptDao, ApplicationDbContext>,
    IAttemptResultRepository
{
    public AttemptResultRepository(ApplicationDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
    }
}
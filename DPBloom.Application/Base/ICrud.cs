using DPBloom.Application.Course;
using DPBloom.Application.Topic;
using DPBloom.Core.Course;

namespace DPBloom.Application.Base;

public interface ICrud<TModel> 
    where TModel : ModelBase<Guid>
{
    Task<IReadOnlyList<TModel>> GetAllAsync();
    Task<TModel> GetByIdAsync(Guid courseId);
    Task<TModel> DeleteAsync(Guid id);
    Task<TModel> RestoreAsync(Guid id);
}
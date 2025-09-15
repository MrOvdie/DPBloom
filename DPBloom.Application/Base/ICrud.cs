using DPBloom.Core.Course;

namespace DPBloom.Application.Base;

public interface ICrud<TModel> 
    where TModel : ModelBase<Guid>
{
    Task<IEnumerable<TModel>> GetAllAsync();
    Task<TModel> GetByIdAsync(Guid id);
    Task<TModel> DeleteAsync(Guid id);
    Task<TModel> RestoreAsync(Guid id);
}
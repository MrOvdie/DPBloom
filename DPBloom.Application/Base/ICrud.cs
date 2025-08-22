using DPBloom.Core.Course;

namespace DPBloom.Application.Base;

public interface ICrud<TModel> 
    where TModel : ModelBase<Guid>
{
    Task<IEnumerable<TModel>> GetAllAsync();
    Task<TModel> GetByIdAsync(string id);
    Task<TModel> DeleteAsync(string id);
    Task<TModel> RestoreAsync(string id);
}
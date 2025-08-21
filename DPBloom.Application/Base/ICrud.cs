namespace DPBloom.Application.Base;

public interface ICrud<TModel> 
    where TModel : ModelBase<Guid>
{
    Task<IEnumerable<TModel>> GetAllAsync();
    Task<TModel> GetByIdAsync(string id);
    Task DeleteAsync(string id);
    Task RestoreAsync(string id);
}
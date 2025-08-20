namespace DPBloom.Application.Base;

public interface ICrud<TModel> 
    where TModel : ModelBase<Guid>
{
    Task<IEnumerable<TModel>> GetAllAsync();

    Task<TModel> GetByIdAsync(string id);

    Task AddAsync(TModel model);

    Task UpdateAsync(TModel model);

    Task DeleteAsync(string modelId);
    Task RestoreAsync(string modelId);
}
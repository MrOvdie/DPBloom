using System.Linq.Expressions;
using DPBloom.Core.Base;

namespace DPBloom.Application.Base;

public interface IRepository<TModel>
{
    Task<IReadOnlyList<TModel>> GetAllAsync();
    Task<IReadOnlyList<TModel>> GetAsync(Expression<Func<TModel, bool>> predicate);
    Task<TModel> GetByIdAsync(Guid id);
    Task<TModel> AddAsync(TModel entity);
    Task<TModel> UpdateAsync(TModel entity);
    Task DeleteAsync(TModel entity);
    Task RestoreAsync(TModel entity);
    Task<bool> ExistsAsync(Expression<Func<TModel, bool>> predicate);
}
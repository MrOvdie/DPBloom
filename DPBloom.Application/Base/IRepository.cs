using System.Linq.Expressions;
using DPBloom.Core.Base;

namespace DPBloom.Application.Base;

public interface IRepository<TModel> where TModel : EntityBase<Guid>
{
    Task<IReadOnlyList<TModel>> GetAllAsync();
    Task<IReadOnlyList<TModel>> GetAsync(Expression<Func<TModel, bool>> predicate);
    Task<TModel> GetByIdAsync(Guid id);
    Task<TModel> AddAsync(TModel entity);
    Task<TModel> UpdateAsync(TModel entity);
    Task DeleteAsync(Guid id);
    Task<TModel> RestoreAsync(Guid id);
    Task<bool> ExistsAsync(Expression<Func<TModel, bool>> predicate);
}
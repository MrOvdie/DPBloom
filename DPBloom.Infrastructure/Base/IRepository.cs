using System.Linq.Expressions;
using DPBloom.Core;
using DPBloom.Core.Specification;

namespace DPBloom.Infrastructure.Base;

public interface IRepository<TEntityReturn, TEntity> 
    where TEntity : EntityDaoBase<Guid>
    where TEntityReturn : EntityBase<Guid>
{
    Task<IReadOnlyList<TEntityReturn>> GetAllAsync();
    Task<IReadOnlyList<TEntityReturn>> GetAsync(Expression<Func<TEntity, bool>> predicate);
    Task<IReadOnlyList<TEntityReturn>> GetAsync(Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string includeString = null,
        bool disableTracking = true);
    Task<IReadOnlyList<TEntityReturn>> GetAsync(Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        List<Expression<Func<TEntity, object>>> includes = null,
        bool disableTracking = true);
    Task<IReadOnlyList<TEntityReturn>> GetAsync(ISpecification<TEntity> spec);
    Task<TEntityReturn> GetByIdAsync(string id);
    Task<TEntityReturn> AddAsync(TEntityReturn entity);
    Task UpdateAsync(TEntityReturn entity);
    Task DeleteAsync(TEntityReturn entity);
    Task RestoreAsync(TEntityReturn entity);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
    Task<int> CountAsync(ISpecification<TEntity> spec);
}
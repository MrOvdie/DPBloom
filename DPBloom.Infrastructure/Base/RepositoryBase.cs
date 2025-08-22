using System.Linq.Expressions;
using AutoMapper;
using DPBloom.Core;
using DPBloom.Core.Specification;
using DPBloom.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Base;

public class RepositoryBase<TEntityReturn, TEntity, TContext> : IRepository<TEntityReturn, TEntity>
    where TEntityReturn : EntityBase<Guid>
    where TEntity : EntityDaoBase<Guid>, ISoftDelete
    where TContext : DbContext
{
    protected readonly TContext DbContext;
    protected readonly IMapper Mapper;

    public RepositoryBase(TContext dbContext, IMapper mapper)
    {
        DbContext = dbContext;
        Mapper = mapper;
    }

    public async Task<IReadOnlyList<TEntityReturn>> GetAllAsync()
    {
        var entities = await DbContext.Set<TEntity>().ToListAsync();
        return Mapper.Map<IReadOnlyList<TEntityReturn>>(entities);
    }

    public async Task<IReadOnlyList<TEntityReturn>> GetAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var entities = await DbContext.Set<TEntity>().Where(predicate).ToListAsync();
        return Mapper.Map<IReadOnlyList<TEntityReturn>>(entities);
    }

    public async Task<IReadOnlyList<TEntityReturn>> GetAsync(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string includeString = null,
        bool disableTracking = true)
    {
        IQueryable<TEntity> query = DbContext.Set<TEntity>();

        if (disableTracking) query = query.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(includeString)) query = query.Include(includeString);
        if (predicate is not null) query = query.Where(predicate);
        if (orderBy is not null) query = orderBy(query);

        var entities = await query.ToListAsync();
        return Mapper.Map<IReadOnlyList<TEntityReturn>>(entities);
    }

    public async Task<IReadOnlyList<TEntityReturn>> GetAsync(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        List<Expression<Func<TEntity, object>>>? includes = null,
        bool disableTracking = true)
    {
        IQueryable<TEntity> query = DbContext.Set<TEntity>();

        if (disableTracking) query = query.AsNoTracking();
        if (includes is not null) query = includes.Aggregate(query, (current, include) => current.Include(include));
        if (predicate is not null) query = query.Where(predicate);
        if (orderBy is not null) query = orderBy(query);

        var entities = await query.ToListAsync();
        return Mapper.Map<IReadOnlyList<TEntityReturn>>(entities);
    }

    public async Task<IReadOnlyList<TEntityReturn>> GetAsync(ISpecification<TEntity> spec)
    {
        var queryableResultWithSpec = await SpecificationEvaluator<TEntity>
            .GetQuery(DbContext.Set<TEntity>().AsQueryable(), spec).ToListAsync();
        return Mapper.Map<IReadOnlyList<TEntityReturn>>(queryableResultWithSpec);
    }

    public async Task<TEntityReturn> GetByIdAsync(string id)
    {
        var entity = await DbContext.Set<TEntity>().AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id.Equals(id));
        return Mapper.Map<TEntityReturn>(entity);
    }

    public async Task<TEntityReturn> AddAsync(TEntityReturn entity)
    {
        var entityDao = Mapper.Map<TEntity>(entity);
        
        await DbContext.Set<TEntity>().AddAsync(entityDao);
        
        if (await DbContext.SaveChangesAsync() > 0)
            return Mapper.Map<TEntityReturn>(entityDao);

        return null;
    }

    public async Task<TEntityReturn> UpdateAsync(TEntityReturn entity)
    {
        //TODO: check, if it working correctly
        var entityDao = Mapper.Map<TEntity>(entity);
        
        DbContext.Entry(entityDao).State = EntityState.Modified;

        if (await DbContext.SaveChangesAsync() > 0)
            return Mapper.Map<TEntityReturn>(entityDao);

        return null;
    }

    public async Task DeleteAsync(TEntityReturn entity)
    {
        /*DbContext.Set<TEntity>().Remove(entity);
        await DbContext.SaveChangesAsync();*/
        var entityDao = Mapper.Map<TEntity>(entity);

        entityDao.Delete();

        DbContext.Entry(entityDao).State = EntityState.Modified;
        await DbContext.SaveChangesAsync();
    }

    public async Task RestoreAsync(TEntityReturn entity)
    {
        //TODO: maybe add some checking, if entity was updated (like in UpdateAsync)
        var entityDao = Mapper.Map<TEntity>(entity);

        entityDao.Undo();

        DbContext.Entry(entityDao).State = EntityState.Modified;
        await DbContext.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await DbContext.Set<TEntity>().AnyAsync(predicate);
    }

    public async Task<int> CountAsync(ISpecification<TEntity> spec)
    {
        return await SpecificationEvaluator<TEntity>
            .GetQuery(DbContext.Set<TEntity>().AsQueryable(), spec).CountAsync();
    }
}
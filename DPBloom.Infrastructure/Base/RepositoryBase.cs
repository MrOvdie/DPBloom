using System.Linq.Expressions;
using AutoMapper;
using DPBloom.Application.Base;
using DPBloom.Core.Base;
using DPBloom.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Base;

public class RepositoryBase<TModel, TDao, TContext> : IRepository<TModel>
    where TDao : EntityDaoBase<Guid>, ISoftDelete
    where TModel : EntityBase<Guid>
    where TContext : DbContext
{
    protected readonly TContext DbContext;
    protected readonly IMapper Mapper;

    protected RepositoryBase(TContext dbContext, IMapper mapper)
    {
        DbContext = dbContext;
        Mapper = mapper;
    }

    public async Task<IReadOnlyList<TModel>> GetAllAsync()
    {
        var entities = await DbContext.Set<TModel>().ToListAsync();
        return entities;
    }
    
    public async Task<IReadOnlyList<TModel>> GetAsync(Expression<Func<TModel, bool>> predicate)
    {
        var entities = await DbContext.Set<TModel>().Where(predicate).ToListAsync();
        return entities;
    }

    public async Task<IReadOnlyList<TModel>> GetAsync(Expression<Func<TModel, bool>>? predicate = null,
        Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null,
        string includeString = null,
        bool disableTracking = true)
    {
        IQueryable<TModel> query = DbContext.Set<TModel>();

        if (disableTracking) query = query.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(includeString)) query = query.Include(includeString);
        if (predicate is not null) query = query.Where(predicate);
        if (orderBy is not null) query = orderBy(query);

        var entities = await query.ToListAsync();
        return entities;
    }

    public async Task<IReadOnlyList<TModel>> GetAsync(Expression<Func<TModel, bool>>? predicate = null,
        Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null,
        List<Expression<Func<TModel, object>>>? includes = null,
        bool disableTracking = true)
    {
        IQueryable<TModel> query = DbContext.Set<TModel>();

        if (disableTracking) query = query.AsNoTracking();
        if (includes is not null) query = includes.Aggregate(query, (current, include) => current.Include(include));
        if (predicate is not null) query = query.Where(predicate);
        if (orderBy is not null) query = orderBy(query);

        var entities = await query.ToListAsync();
        return entities;
    }

    /*public async Task<IReadOnlyList<TEntityReturn>> GetAsync(ISpecification<TEntity> spec)
    {
        var queryableResultWithSpec = await SpecificationEvaluator<TEntity>
            .GetQuery(DbContext.Set<TEntity>().AsQueryable(), spec).ToListAsync();
        return Mapper.Map<IReadOnlyList<TEntityReturn>>(queryableResultWithSpec);
    }*/

    public async Task<TModel> GetByIdAsync(Guid id)
    {
        var entity = await DbContext.Set<TModel>().AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id.Equals(id));
        return entity;
    }

    public async Task<TModel> AddAsync(TModel entity)
    {
        var entityDao = Mapper.Map<TModel>(entity);

        await DbContext.Set<TModel>().AddAsync(entityDao);

        await DbContext.SaveChangesAsync();
        return entityDao;
    }

    public async Task<TModel> UpdateAsync(TModel entity)
    {
        //TODO: check, if it working correctly
        var entityDao = Mapper.Map<TModel>(entity);
        
        DbContext.Update(entityDao);
        
        await DbContext.SaveChangesAsync();
        
        return Mapper.Map<TModel>(entityDao);
        /*var entityDao = Mapper.Map<TEntity>(entity);

        DbContext.Entry(entityDao).State = EntityState.Modified;

        await DbContext.SaveChangesAsync();

        return Mapper.Map<TEntityReturn>(entityDao);*/
    }

    public async Task DeleteAsync(TModel entity)
    {
        /*DbContext.Set<TEntity>().Remove(entity);
        await DbContext.SaveChangesAsync();*/
        var entityDao = Mapper.Map<TDao>(entity);

        entityDao.Delete();
        
        await DbContext.SaveChangesAsync();
    }

    public async Task RestoreAsync(TModel entity)
    {
        //TODO: maybe add some checking, if entity was updated (like in UpdateAsync)
        var entityDao = Mapper.Map<TDao>(entity);

        entityDao.Undo();

        //DbContext.Entry(entityDao).State = EntityState.Modified;
        await DbContext.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<TModel, bool>> predicate)
    {
        return await DbContext.Set<TModel>().AnyAsync(predicate);
    }

    /*public async Task<int> CountAsync(ISpecification<TEntity> spec)
    {
        return await SpecificationEvaluator<TEntity>
            .GetQuery(DbContext.Set<TEntity>().AsQueryable(), spec).CountAsync();
    }*/
}
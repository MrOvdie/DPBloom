using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
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

    public async Task<IReadOnlyList<TModel>> GetAllAsync(int pageNumber, int pageSize)
    {
        /*var entitiesDao = DbContext.Set<TDao>();
        
        var totalCount = await entitiesDao.CountAsync();
        
        var items = await entitiesDao
            .OrderByDescending(ar => ar.CreatedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ar => Mapper.Map<TModel>(ar))
            .ToListAsync();

        return new Application.Base.PagedResult<TModel>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };*/
        var entitiesDao = await DbContext.Set<TDao>().ToListAsync();

        var entities = Mapper.Map<List<TModel>>(entitiesDao);

        return entities;
    }
    
    public async Task<IReadOnlyList<TModel>> GetAsync(Expression<Func<TModel, bool>> predicate)
    {
        var mappedPredicate = Mapper.Map<Expression<Func<TDao, bool>>>(predicate);
        
        var entitiesDao = await DbContext.Set<TDao>().Where(mappedPredicate).ToListAsync();
        
        var entities = Mapper.Map<List<TModel>>(entitiesDao);
        
        return entities;
    }

    public async Task<IReadOnlyList<TModel>> GetAsync(Expression<Func<TModel, bool>>? predicate = null,
        string? orderBy = null,
        string? includeString = null,
        bool disableTracking = true)
    {
        IQueryable<TDao> query = DbContext.Set<TDao>();

        if (disableTracking) query = query.AsNoTracking();
        
        if (!string.IsNullOrWhiteSpace(includeString)) query = query.Include(includeString);
        
        if (predicate is not null)
        {
            var mappedPredicate = Mapper.Map<Expression<Func<TDao, bool>>>(predicate);
            query = query.Where(mappedPredicate);
        }
        
        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            query = query.OrderBy(orderBy); 
        }

        var entitiesDao = await query.ToListAsync();
        
        var entities = Mapper.Map<List<TModel>>(entitiesDao);
        
        return entities;
    }

    public async Task<IReadOnlyList<TModel>> GetAsync(Expression<Func<TModel, bool>>? predicate = null,
        string? orderBy = null,
        List<Expression<Func<TModel, object>>>? includes = null,
        bool disableTracking = true)
    {
        IQueryable<TModel> query = DbContext.Set<TModel>();

        if (disableTracking) query = query.AsNoTracking();
        
        if (includes is not null) query = includes.Aggregate(query, (current, include) => current.Include(include));
       
        if (predicate is not null)
        {
            var mappedPredicate = Mapper.Map<Expression<Func<TDao, bool>>>(predicate);
            query = query.Where(mappedPredicate);
        }
        
        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            query = query.OrderBy(orderBy); 
        }

        var entitiesDao = await query.ToListAsync();
        
        var entities = Mapper.Map<List<TModel>>(entitiesDao);
        
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
        var entityDao = await DbContext.Set<TDao>().AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id.Equals(id));
        
        var entity = Mapper.Map<TModel>(entityDao);
        
        return entity;
    }

    public async Task<TModel> AddAsync(TModel entity)
    {
        var entityDao = Mapper.Map<TDao>(entity);

        await DbContext.Set<TDao>().AddAsync(entityDao);

        await DbContext.SaveChangesAsync();
        
        var enitityModel = Mapper.Map<TModel>(entityDao);
        
        return enitityModel;
    }

    public async Task<TModel> UpdateAsync(TModel entity)
    {
        var dao = Mapper.Map<TDao>(entity);

        DbContext.ChangeTracker.Clear();

        DbContext.Set<TDao>().Update(dao);

        await DbContext.SaveChangesAsync();

        return Mapper.Map<TModel>(dao);
        
        /*//TODO: check, if it working correctly
        
        var existingDao = await DbContext.Set<TDao>().FindAsync(entity.Id); 
        
        if (existingDao is null)
        {
            return null; 
        }

        Mapper.Map(entity, existingDao);
    
        await DbContext.SaveChangesAsync();
    
        return Mapper.Map<TModel>(existingDao);*/
    }

    public async Task DeleteAsync(Guid id)
    {
        /*DbContext.Set<TEntity>().Remove(entity);
        await DbContext.SaveChangesAsync();*/
        var existingDao = await DbContext.Set<TDao>().FindAsync(id);

        if (existingDao is null)
        {
            return;
        }

        if (existingDao is ISoftDelete softDeleteEntity)
        {
            softDeleteEntity.Delete(); //TODO: Check this
            
            DbContext.Entry(existingDao).State = EntityState.Modified;
        }
        else
        {
            DbContext.Set<TDao>().Remove(existingDao);
        }
        
        await DbContext.SaveChangesAsync();
    }

    public async Task<TModel> RestoreAsync(Guid id)
    {
        var existingDao = await DbContext.Set<TDao>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        
        if (existingDao is null)
        {
            return null;
        }
        
        if (existingDao is ISoftDelete softDeletable) 
        {
            softDeletable.Undo();
            await DbContext.SaveChangesAsync();
        }
        
        return Mapper.Map<TModel>(existingDao);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TModel, bool>> predicate)
    {
        var mappedPredicate = Mapper.Map<Expression<Func<TDao, bool>>>(predicate);
        
        return await DbContext.Set<TDao>().AnyAsync(mappedPredicate);
    }

    /*public async Task<int> CountAsync(ISpecification<TEntity> spec)
    {
        return await SpecificationEvaluator<TEntity>
            .GetQuery(DbContext.Set<TEntity>().AsQueryable(), spec).CountAsync();
    }*/
}
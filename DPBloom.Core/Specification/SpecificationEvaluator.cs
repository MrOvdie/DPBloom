using Microsoft.EntityFrameworkCore;

namespace DPBloom.Core.Specification;

public static class SpecificationEvaluator<TEntity> where TEntity : class
{
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> spec)
    {
        var query = inputQuery;

        // WHERE
        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }

        // Include (expression)
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Include (string)
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        // ORDER BY
        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        // Paging
        if (spec.isPagingEnabled)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }

        return query;
    }
}
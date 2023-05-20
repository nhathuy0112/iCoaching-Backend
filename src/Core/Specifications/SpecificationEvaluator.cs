using Core.Entities;
using Microsoft.EntityFrameworkCore;
namespace Core.Specifications
{
    public class SpecificationEvaluator<TEntity> where TEntity : class
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> specification)
        {
            var query = inputQuery;

            if (specification.Criteria != null)
            {
                query = query
                    .Where(specification.Criteria);
            }

            if (specification.OrderBy != null)
            {
                query = query
                    .OrderBy(specification.OrderBy);
            }

            if (specification.OrderByDescending != null)
            {
                query = query
                    .OrderByDescending(specification.OrderByDescending);
            }

            if (specification.IsPagingEnabled)
            {
                query = query
                    .Skip(specification.Skip)
                    .Take(specification.Take);
            }

            query = specification.Includes.Aggregate(query, (current, include) =>
                current.Include(include)
            );
            query = specification.IncludeStrings.Aggregate(query, (current, include) => 
                current.Include(include));
            return query;
        }
    }
}

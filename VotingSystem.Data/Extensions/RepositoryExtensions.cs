using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Pagination;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace VotingSystem.Data.Extensions
{
    public static class RepositoryExtensions
    {
        public static IQueryable<T> Sort<T>(this IQueryable<T> query, string orderByQueryString)
        {
            if (string.IsNullOrWhiteSpace(orderByQueryString))
                return query;

            string orderQuery = OrderQueryBuilder.CreateOrderQuery<T>(orderByQueryString);

            if (string.IsNullOrWhiteSpace(orderQuery))
                return query;

            return query.OrderBy(orderQuery);
        }

        public static async Task<PagedList<T>> GetPagedItems<T>(this IQueryable<T> query, RequestParameters parameters, Expression<Func<T, bool>>? searchExpression = null)
        {
            if (searchExpression != null)
                query = query.Where(searchExpression);

            if (!string.IsNullOrWhiteSpace(parameters.OrderBy))
                query = query.Sort(parameters.OrderBy);

            long count = await query.LongCountAsync();
            int skip = (parameters.PageNumber - 1) * parameters.PageSize;

            List<T> items = await query
                .Skip(skip)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedList<T>(items, count, parameters.PageNumber, parameters.PageSize);
        }
    }
}
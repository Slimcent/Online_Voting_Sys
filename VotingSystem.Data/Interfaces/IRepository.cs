using Microsoft.EntityFrameworkCore.Query;
using OnlineVoting.Models.Pagination;
using System.Linq.Expressions;

namespace VotingSystem.Data.Interfaces
{
    public interface IRepository<T>
    {
        T Add(T obj);
        Task<T> AddAsync(T obj);
        void AddRange(IList<T> obj);
        Task AddRangeAsync(IList<T> obj);
        Task DeleteByIdAsync(object id);
        bool Delete(Expression<Func<T, bool>> predicate);
        bool Delete(T obj);
        Task DeleteAsync(Expression<Func<T, bool>> predicate);
        Task DeleteAsync(T obj);
        bool DeleteById(object id);  
        bool DeleteRange(Expression<Func<T, bool>> predicate);
        bool DeleteRange(IEnumerable<T> records);
        Task DeleteRangeAsync(Expression<Func<T, bool>> predicate);
        Task DeleteRangeAsync(IEnumerable<T> records);
        T Update(T obj);
        Task<T> UpdateAsync(T obj);
        long Count(Expression<Func<T, bool>> predicate = null);
        Task<long> CountAsync(Expression<Func<T, bool>> predicate = null);
        Task<decimal> SumAsync(Expression<Func<T, decimal>> predicate);
        Task<int> SumAsync(Expression<Func<T, int>> predicate);
        Task<long> SumAsync(Expression<Func<T, long>> predicate);
        T GetById(object id);
        Task<T> GetByIdAsync(object id);
        IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();
        T FindOneInclude(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
        Task<IEnumerable<T>> GetAllAndInclude(params Expression<Func<T, object>>[] includeProperties);
        Task<IEnumerable<T>> GetByAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int? skip = null, int? take = null, params string[] includeProperties);
        Task<IEnumerable<T>> GetByAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int? skip = null, int? take = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);
        T GetSingleBy(Expression<Func<T, bool>> predicate);
        Task<T> GetSingleByAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetSingleByAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int? skip = null, int? take = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool tracking = false);
        Task<PagedList<T>> GetPagedItems(RequestParameters parameters, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);
        int Save();
        Task<int> SaveAsync();
        void Dispose();
    }
}

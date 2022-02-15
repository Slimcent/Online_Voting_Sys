using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using VotingSystem.Data.Interfaces;

namespace VotingSystem.Data.Implementation
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _dbContext = context ?? throw new ArgumentException(nameof(context));
            _dbSet = _dbContext.Set<T>();
        }

        public T Add(T obj)
        {
            try
            {
                _dbSet.Add(obj);
                return obj;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<T> AddAsync(T obj)
        {
            Add(obj);
            await _dbContext.SaveChangesAsync();
            return obj;
        }

        public void AddRange(IList<T> obj)
        {
            try
            {
                _dbSet.AddRange(obj);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task AddRangeAsync(IList<T> obj)
        {
            AddRange(obj);
            await SaveAsync();
        }

        public virtual bool Delete(T obj)
        {
            try
            {
                _dbSet.Remove(obj);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public virtual bool Delete(Expression<Func<T, bool>> predicate)
        {
            try
            {
                var obj = GetSingleBy(predicate);
                if (obj != null)
                {
                    _dbSet.Remove(obj);
                    return true;
                }
                else
                    throw new Exception($"object does not exist");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public virtual async Task DeleteAsync(T obj)
        {
            Delete(obj);
            await SaveAsync();
        }

        public virtual async Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            Delete(predicate);
            await SaveAsync();
        }

        public virtual bool DeleteById(object id)
        {
            try
            {
                var obj = _dbSet.Find(id);
                if (obj != null)
                {
                    _dbSet.Remove(obj);
                    return true;
                }
                else
                    throw new Exception($"object with id {id} does not exist");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public virtual async Task DeleteByIdAsync(object id)
        {
            DeleteById(id);
            await SaveAsync();
        }


        public virtual bool DeleteRange(Expression<Func<T, bool>> predicate)
        {
            try
            {
                IEnumerable<T> records = from x in _dbSet.Where<T>(predicate) select x;
                _dbSet.RemoveRange(records);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual bool DeleteRange(IEnumerable<T> records)
        {
            try
            {
                _dbSet.RemoveRange(records);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> records)
        {
            DeleteRange(records);
            await SaveAsync();
        }
        public async Task DeleteRangeAsync(Expression<Func<T, bool>> predicate)
        {
            DeleteRange(predicate);
            await SaveAsync();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public T FindOneInclude(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            var query = GetAllIncluding(includeProperties);
            T obj = query.FirstOrDefault(predicate);

            return obj;
        }

        public IEnumerable<T> GetAll()
        {
            try
            {
                return _dbSet.ToList();
            }
            catch (Exception ex)
            {
                throw  ex;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAndInclude(params Expression<Func<T, object>>[] includeProperties)
        {
            var query = GetAllIncluding(includeProperties);
            IEnumerable<T> results = await query.ToListAsync();

            return results;
        }

        public virtual async Task<IEnumerable<T>> GetByAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int? skip = null, int? take = null, params string[] includeProperties)
        {
            try
            {
                IQueryable<T> query = constructQuery(predicate, orderBy, skip, take, includeProperties);

                return await query.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetByAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int? skip = null, int? take = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            try
            {
                IQueryable<T> query = constructQuery(predicate, orderBy, skip, take, include);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public T GetById(object id)
        {
            return _dbSet.Find(id);
        }

        public async Task<T> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

       
        public int Save()
        {
            try
            {
                return _dbContext.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<int> SaveAsync()
        {
            try
            {
                return _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public T Update(T obj)
        {
            try
            {
                _dbSet.Attach(obj);
                _dbContext.Entry<T>(obj).State = EntityState.Modified;
                return obj;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public  async Task<T> UpdateAsync(T obj)
        {
            Update(obj);
            await SaveAsync();
            return obj;
        }

        public virtual async Task<T> GetSingleByAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<T> GetSingleByAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int? skip = null, int? take = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool tracking = false)
        {
            try
            {
                IQueryable<T> query = constructQuery(predicate, orderBy, skip, take, include);
                if (!tracking)
                    return await query.AsNoTracking().FirstOrDefaultAsync();
                return await query.SingleAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public T GetSingleBy(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.FirstOrDefault(predicate);
        }

        public async Task<decimal> SumAsync(Expression<Func<T, decimal>> predicate)
        {
            return await _dbSet.SumAsync(predicate);
        }

        public async Task<int> SumAsync(Expression<Func<T, int>> predicate)
        {
            return await _dbSet.SumAsync(predicate);
        }

        public async Task<long> SumAsync(Expression<Func<T, long>> predicate)
        {
            return await _dbSet.SumAsync(predicate);
        }

        public virtual long Count(Expression<Func<T, bool>> predicate = null)
        {
            try
            {
                if (predicate == null)
                    return _dbSet.LongCount();
                return _dbSet.LongCount(predicate);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<long> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            try
            {
                if (predicate == null)
                    return await _dbSet.LongCountAsync();
                return await _dbSet.LongCountAsync(predicate);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private IQueryable<T> GetAllIncluding(Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> queryable = _dbContext.Set<T>();

            return includeProperties.Aggregate(queryable,
                (current, includeProperty) => current.Include(includeProperty));
        }

        private IQueryable<T> constructQuery(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, int? skip, int? take, params string[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            for (int i = 0; i < includeProperties.Length; i++)
            {
                query = query.Include(includeProperties[i]);
            }

            if (skip != null)
            {
                query = query.Skip(skip.Value);
            }

            if (take != null)
            {
                query = query.Take(take.Value);
            }

            return query;
        }

        private IQueryable<T> constructQuery(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, int? skip, int? take, Func<IQueryable<T>, IIncludableQueryable<T, object>> include)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (include != null) query = include(query);

            if (skip != null)
            {
                query = query.Skip(skip.Value);
            }

            if (take != null)
            {
                query = query.Take(take.Value);
            }

            return query;
        }
    }
}

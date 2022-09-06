namespace OnlineVoting.Data.Interfaces
{
    public interface IRepository<T>
    {
        T Add(T obj);
        Task<T> AddAsync(T obj);
        void AddRange(IList<T> obj);
    }
}

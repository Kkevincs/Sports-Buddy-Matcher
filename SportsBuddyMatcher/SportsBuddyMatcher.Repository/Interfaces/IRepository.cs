using System.Linq.Expressions;

namespace SportsBuddyMatcher.Repository.Interfaces;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task AddRangeAsync(List<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

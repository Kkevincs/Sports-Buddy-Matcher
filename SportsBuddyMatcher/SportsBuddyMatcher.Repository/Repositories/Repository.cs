using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SportsBuddyMatcher.Repository.Interfaces;

namespace SportsBuddyMatcher.Repository.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly SportsBuddyMatcherDbContext _context;

    public Repository(SportsBuddyMatcherDbContext context)
    {
        _context = context;
    }

    public Task<List<T>> GetAllAsync()
    {
        return _context.Set<T>().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().Where(predicate).ToListAsync();
    }

    public Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().FirstOrDefaultAsync(predicate);
    }

    public async Task AddAsync(T entity)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(List<T> entities)
    {
        _context.Set<T>().AddRange(entities);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity is null)
            return;

        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
    }
}

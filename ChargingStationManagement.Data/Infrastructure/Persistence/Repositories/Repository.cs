using System.Linq.Expressions;
using ChargingStationManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingStationManagement.Infrastructure.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // 基础 GetByIdAsync
    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    // 🔥 新增：带 Include 的 GetByIdAsync 重载
    public virtual async Task<T?> GetByIdAsync(Guid id, Func<IQueryable<T>, IQueryable<T>>? include = null)
    {
        IQueryable<T> query = _dbSet;

        if (include != null)
        {
            query = include(query);
        }

        // 假设实体有 Id 属性
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, "Id");
        var constant = Expression.Constant(id);
        var equals = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

        return await query.FirstOrDefaultAsync(lambda);
    }

    // FirstOrDefaultAsync 保持不变
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    // 🔥 修复：返回类型改为 Task<List<T>>
    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    // GetAsync 保持不变（这个不在接口中）
    public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    // 🔥 新增：FindAsync 方法
    public virtual async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    // 🔥 修复：返回类型改为 Task<T>
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // 🔥 修复：添加 SaveChanges
    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    // 🔥 修复：添加 SaveChanges
    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    // 🔥 新增：ExistsAsync 方法
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    // 🔥 新增：Query 方法
    public virtual IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

    // 保留这个方法但标记为过时或删除（因为不在接口中）
    [Obsolete("Use GetByIdAsync with Guid parameter instead")]
    public Task<T> GetByIdAsync(string operatorId)
    {
        throw new NotImplementedException("This method is not supported. Use GetByIdAsync(Guid id) instead.");
    }
}
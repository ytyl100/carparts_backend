// ChargingStationManagement.Domain/Interfaces/IRepository.cs
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Specifications;

namespace ChargingStationManagement.Domain.Interfaces
{
    /// <summary>
    /// 泛型仓储接口
    /// </summary>
    /// <typeparam name="T">聚合根类型</typeparam>
    public interface IRepository<T> where T : AggregateRoot
    {
        Task<T> GetByIdAsync(Guid id);
        Task<T> GetByIdAsync(string externalId);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate);
        Task<IReadOnlyList<T>> GetAsync(ISpecification<T> specification);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<T> FirstOrDefaultAsync(ISpecification<T> specification);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(ISpecification<T> specification);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task GetByIdAsync(object stationId);
    }
}
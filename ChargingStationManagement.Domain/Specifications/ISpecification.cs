// ChargingStationManagement.Domain/Specifications/ISpecification.cs
using System;
using System.Linq.Expressions;

namespace ChargingStationManagement.Domain.Specifications
{
    /// <summary>
    /// 规约接口
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        List<string> IncludeStrings { get; }
        Expression<Func<T, object>> OrderBy { get; }
        Expression<Func<T, object>> OrderByDescending { get; }
        int Take { get; }
        int Skip { get; }
        bool IsPagingEnabled { get; }
    }
}
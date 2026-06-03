// ChargingStationManagement.Domain/Specifications/UserSpecifications.cs
using System;
using System.Linq.Expressions;
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;

namespace ChargingStationManagement.Domain.Specifications
{
    // 将 Criteria 的赋值改为通过构造函数传递给 BaseSpecification
    public class ActiveUsersSpecification : BaseSpecification<User>
    {
        public ActiveUsersSpecification(bool includeWallet = false, bool includeVehicles = false)
            : base(u => u.IsActive && u.IsVerified)
        {
            if (includeWallet)
            {
                AddInclude(u => u.Wallet);
            }

            if (includeVehicles)
            {
                AddInclude(u => u.Vehicles);
            }

            ApplyOrderBy(u => u.Name);
        }
    }

    public class UserByIdSpecification : BaseSpecification<User>
    {
        public UserByIdSpecification(string userId, bool includeWallet = true, bool includeVehicles = true)
            : base(u => u.UserId == userId)
        {
            if (includeWallet)
            {
                AddInclude(u => u.Wallet);
            }

            if (includeVehicles)
            {
                AddInclude(u => u.Vehicles);
            }
        }
    }

    public class UsersByTypeSpecification : BaseSpecification<User>
    {
        public UsersByTypeSpecification(UserType userType, bool activeOnly = true)
            : base(activeOnly
                ? (Expression<Func<User, bool>>)(u => u.UserType == userType && u.IsActive && u.IsVerified)
                : (u => u.UserType == userType))
        {
            ApplyOrderBy(u => u.RegistrationDate);
        }
    }

    public class UsersWithLowBalanceSpecification : BaseSpecification<User>
    {
        public UsersWithLowBalanceSpecification(decimal threshold = 10, bool includeWallet = true)
            : base(null)
        {
            // 需要先Include钱包才能查询余额
            // 这个规约需要在Repository中特殊处理
            // 这里只是标记，实际查询可能需要JOIN

            if (includeWallet)
            {
                AddInclude(u => u.Wallet);
            }

            // Criteria将在Repository中构建
        }
    }

    public class UsersByRegistrationDateSpecification : BaseSpecification<User>
    {
        public UsersByRegistrationDateSpecification(DateTime startDate, DateTime endDate)
            : base(u => u.RegistrationDate >= startDate && u.RegistrationDate <= endDate)
        {
            AddInclude(u => u.Wallet);

            ApplyOrderByDescending(u => u.RegistrationDate);
        }
    }
}
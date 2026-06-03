// ChargingStationManagement.Domain/Entities/User.cs
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Events;
using System;
using System.Collections.Generic;
using static System.Collections.Specialized.BitVector32;

namespace ChargingStationManagement.Domain.Entities
{
    /// <summary>
    /// 用户实体（聚合根）
    /// </summary>
    public class User : AggregateRoot
    {
        // 用户标识
        public string UserId { get; private set; }               // 用户唯一ID
        public string PhoneNumber { get; private set; }          // 手机号（登录账号）
        public string Email { get; private set; }                // 邮箱
        public string Name { get; private set; }                 // 姓名
        public string IdentityNumber { get; private set; }       // 身份证号

        // 用户信息
        public UserType UserType { get; private set; }           // 用户类型
        public string AvatarUrl { get; private set; }            // 头像URL
        public DateTime? DateOfBirth { get; private set; }       // 出生日期
        public Gender Gender { get; private set; }               // 性别
        public string EmergencyContact { get; private set; }     // 紧急联系人
        public string EmergencyPhone { get; private set; }       // 紧急联系电话

        // 账户状态
        public bool IsActive { get; private set; }               // 是否激活
        public bool IsVerified { get; private set; }             // 是否已验证
        public DateTime? VerificationDate { get; private set; }  // 验证日期
        public string VerificationMethod { get; private set; }   // 验证方式

        // 安全信息
        public string PasswordHash { get; private set; }         // 密码哈希
        public string Salt { get; private set; }                 // 密码盐
        public DateTime? LastLoginTime { get; private set; }     // 最后登录时间
        public string LastLoginIp { get; private set; }          // 最后登录IP
        public int FailedLoginAttempts { get; private set; }     // 登录失败次数
        public DateTime? LockoutEndTime { get; private set; }    // 锁定结束时间

        // 偏好设置
        public string Language { get; private set; }             // 语言偏好
        public string Timezone { get; private set; }             // 时区
        public NotificationPreferences NotificationPrefs { get; private set; } // 通知偏好

        // 统计信息
        public int TotalSessions { get; private set; }           // 总充电次数
        public decimal TotalEnergyConsumed { get; private set; } // 总耗电量（kWh）
        public decimal TotalAmountSpent { get; private set; }    // 总消费金额
        public DateTime? LastChargingTime { get; private set; }  // 最后充电时间

        // 注册信息
        public DateTime RegistrationDate { get; private set; }   // 注册日期
        public string RegistrationSource { get; private set; }   // 注册来源（App/Web/WeChat）
        public string ReferralCode { get; private set; }         // 推荐码
        public string ReferredBy { get; private set; }           // 被谁推荐

        // 导航属性
        private Wallet _wallet;                                  // 钱包
        private readonly List<Vehicle> _vehicles = new List<Vehicle>(); // 车辆列表
        private readonly List<Session> _sessions = new List<Session>(); // 充电会话列表
        private readonly List<UserFavorite> _favorites = new List<UserFavorite>(); // 收藏列表

        public Wallet Wallet => _wallet;
        public IReadOnlyCollection<Vehicle> Vehicles => _vehicles.AsReadOnly();
        public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();
        public IReadOnlyCollection<UserFavorite> Favorites => _favorites.AsReadOnly();

        // 构造函数
        protected User() { }

        public User(
            string userId,
            string phoneNumber,
            string name,
            UserType userType = UserType.Normal,
            string createdBy = "system")
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

            UserId = userId;
            PhoneNumber = phoneNumber;
            Name = name ?? "Anonymous User";
            UserType = userType;

            IsActive = true;
            RegistrationDate = DateTime.UtcNow;
            RegistrationSource = "System";

            // 创建默认钱包
            _wallet = new Wallet(Id, userId);

            // 设置默认偏好
            Language = "zh-CN";
            Timezone = "China Standard Time";
            NotificationPrefs = new NotificationPreferences(true, true, true, false);

            CreatedBy = createdBy;

            AddDomainEvent(new UserRegisteredEvent(Id, userId, phoneNumber, name));
        }

        // 业务方法
        public void UpdateProfile(
            string email,
            string name,
            DateTime? dateOfBirth,
            Gender gender,
            string emergencyContact,
            string emergencyPhone)
        {
            Email = email;
            Name = name ?? Name;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            EmergencyContact = emergencyContact;
            EmergencyPhone = emergencyPhone;

            Update();

            AddDomainEvent(new UserProfileUpdatedEvent(Id, UserId));
        }

        public void SetPassword(string passwordHash, string salt)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

            PasswordHash = passwordHash;
            Salt = salt;

            Update();
        }

        public void UpdatePreferences(string language, string timezone, NotificationPreferences prefs)
        {
            Language = language ?? Language;
            Timezone = timezone ?? Timezone;
            NotificationPrefs = prefs ?? NotificationPrefs;

            Update();
        }

        public void AddVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
                throw new ArgumentNullException(nameof(vehicle));

            if (_vehicles.Any(v => v.LicensePlate == vehicle.LicensePlate))
                throw new InvalidOperationException($"Vehicle with license plate {vehicle.LicensePlate} already exists");

            _vehicles.Add(vehicle);
            Update();

            AddDomainEvent(new VehicleAddedEvent(Id, UserId, vehicle.Id, vehicle.LicensePlate));
        }

        public void RemoveVehicle(Guid vehicleId)
        {
            var vehicle = _vehicles.FirstOrDefault(v => v.Id == vehicleId);
            if (vehicle != null)
            {
                _vehicles.Remove(vehicle);
                Update();

                AddDomainEvent(new VehicleRemovedEvent(Id, UserId, vehicleId, vehicle.LicensePlate));
            }
        }

        public void AddFavorite(string stationId, string stationName)
        {
            if (string.IsNullOrWhiteSpace(stationId))
                throw new ArgumentException("Station ID cannot be empty", nameof(stationId));

            if (_favorites.Any(f => f.StationId == stationId))
                throw new InvalidOperationException($"Station {stationId} is already in favorites");

            var favorite = new UserFavorite(Id, stationId, stationName);
            _favorites.Add(favorite);
            Update();

            AddDomainEvent(new StationFavoritedEvent(Id, UserId, stationId, stationName));
        }

        public void RemoveFavorite(string stationId)
        {
            var favorite = _favorites.FirstOrDefault(f => f.StationId == stationId);
            if (favorite != null)
            {
                _favorites.Remove(favorite);
                Update();

                AddDomainEvent(new StationUnfavoritedEvent(Id, UserId, stationId));
            }
        }

        public void RecordLogin(string ipAddress)
        {
            LastLoginTime = DateTime.UtcNow;
            LastLoginIp = ipAddress;
            FailedLoginAttempts = 0; // 重置失败次数
            LockoutEndTime = null;   // 解除锁定

            Update();
        }

        public void RecordFailedLogin()
        {
            FailedLoginAttempts++;

            // 如果连续失败5次，锁定账户1小时
            if (FailedLoginAttempts >= 5)
            {
                LockoutEndTime = DateTime.UtcNow.AddHours(1);
            }

            Update();
        }

        public void VerifyAccount(string method = "Phone")
        {
            if (!IsVerified)
            {
                IsVerified = true;
                VerificationDate = DateTime.UtcNow;
                VerificationMethod = method;

                Update();

                AddDomainEvent(new UserVerifiedEvent(Id, UserId, method));
            }
        }

        public void Activate()
        {
            if (!IsActive)
            {
                IsActive = true;
                Update();

                AddDomainEvent(new UserActivatedEvent(Id, UserId));
            }
        }

        public void Deactivate(string reason = null)
        {
            if (IsActive)
            {
                IsActive = false;
                Update();

                AddDomainEvent(new UserDeactivatedEvent(Id, UserId, reason));
            }
        }

        public void UpdateStatistics(decimal energy, decimal amount)
        {
            TotalSessions += 1;
            TotalEnergyConsumed += energy;
            TotalAmountSpent += amount;
            LastChargingTime = DateTime.UtcNow;

            Update();
        }

        // 验证方法
        public bool IsLockedOut()
        {
            return LockoutEndTime.HasValue && LockoutEndTime.Value > DateTime.UtcNow;
        }

        public bool CanStartCharging()
        {
            return IsActive && IsVerified && !IsLockedOut() && Wallet.Balance > 0;
        }
    }

    /// <summary>
    /// 性别枚举
    /// </summary>
    public enum Gender
    {
        Unknown = 0,
        Male = 1,
        Female = 2,
        Other = 3
    }
}
// ChargingStationManagement.Infrastructure/Persistence/Repositories/UserRepository.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingStationManagement.Infrastructure.Persistence.Repositories
{
    public class UserRepository : Repository<User>, IRepository<User>
    {
        public UserRepository(ChargingStationDbContext context) : base(context)
        {
        }

        public override async Task<User> GetByIdAsync(string externalId)
        {
            return await _dbSet
                .Include(u => u.Wallet)
                .Include(u => u.Vehicles)
                .FirstOrDefaultAsync(u => u.UserId == externalId);
        }

        public async Task<User> GetUserByPhoneAsync(string phoneNumber)
        {
            return await _dbSet
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IReadOnlyList<User>> GetActiveUsersAsync()
        {
            return await _dbSet
                .Include(u => u.Wallet)
                .Where(u => u.IsActive && u.IsVerified)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<User>> GetUsersByTypeAsync(UserType userType)
        {
            return await _dbSet
                .Include(u => u.Wallet)
                .Where(u => u.UserType == userType)
                .OrderBy(u => u.RegistrationDate)
                .ToListAsync();
        }

        public async Task<User> GetUserWithWalletAndVehiclesAsync(string userId)
        {
            return await _dbSet
                .Include(u => u.Wallet)
                .Include(u => u.Vehicles)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task UpdateUserLoginInfoAsync(Guid userId, string ipAddress)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.RecordLogin(ipAddress);
                await UpdateAsync(user);
            }
        }

        public async Task RecordFailedLoginAsync(Guid userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.RecordFailedLogin();
                await UpdateAsync(user);
            }
        }

        public async Task<bool> CheckIfPhoneExistsAsync(string phoneNumber, Guid? excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.PhoneNumber == phoneNumber);

            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> CheckIfEmailExistsAsync(string email, Guid? excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.Email == email && !string.IsNullOrEmpty(email));

            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IReadOnlyList<User>> GetUsersWithLowBalanceAsync(decimal threshold = 10)
        {
            return await _dbSet
                .Include(u => u.Wallet)
                .Where(u => u.Wallet.Balance < threshold && u.IsActive)
                .ToListAsync();
        }
    }
}
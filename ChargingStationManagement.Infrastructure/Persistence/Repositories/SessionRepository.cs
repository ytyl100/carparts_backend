using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingStationManagement.Infrastructure.Persistence.Repositories
{
    public class SessionRepository : Repository<Session>, IRepository<Session>
    {
        public SessionRepository(ChargingStationDbContext context) : base(context)
        {
        }

        public override async Task<Session> GetByIdAsync(string externalId)
        {
            return await _dbSet
                .Include(s => s.User)
                .Include(s => s.Connector)
                .Include(s => s.Station)
                .FirstOrDefaultAsync(s => s.SessionId == externalId);
        }

        public async Task<Session> GetSessionByChargeSeqAsync(string startChargeSeq)
        {
            return await _dbSet
                .Include(s => s.User)
                .Include(s => s.Connector)
                .Include(s => s.Station)
                .FirstOrDefaultAsync(s => s.StartChargeSeq == startChargeSeq);
        }

        public async Task<IReadOnlyList<Session>> GetActiveSessionsAsync()
        {
            return await _dbSet
                .Include(s => s.User)
                .Include(s => s.Connector)
                .Include(s => s.Station)
                .Where(s => s.Status == ChargeStatus.Charging || s.Status == ChargeStatus.Starting)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Session>> GetUserSessionsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(s => s.Connector)
                .Include(s => s.Station)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Session>> GetStationSessionsAsync(Guid stationId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet
                .Include(s => s.User)
                .Include(s => s.Connector)
                .Where(s => s.StationId == stationId);

            if (startDate.HasValue)
            {
                query = query.Where(s => s.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(s => s.StartTime <= endDate.Value);
            }

            return await query
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Session>> GetUnpaidSessionsAsync(DateTime? olderThan = null)
        {
            var query = _dbSet
                .Include(s => s.User)
                .Where(s => !s.IsPaid && s.OrderStatus == OrderStatus.Completed && s.TotalAmount > 0);

            if (olderThan.HasValue)
            {
                query = query.Where(s => s.EndTime <= olderThan.Value);
            }

            return await query
                .OrderBy(s => s.EndTime)
                .ToListAsync();
        }

        public async Task UpdateSessionStatusAsync(Guid sessionId, ChargeStatus status)
        {
            var session = await GetByIdAsync(sessionId);
            if (session != null)
            {
                // 注意：这里只是简单更新状态，实际应该通过领域方法
                var property = _context.Entry(session).Property("Status");
                property.CurrentValue = (int)status;
                property.IsModified = true;

                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkSessionAsPaidAsync(Guid sessionId, string transactionId)
        {
            var session = await GetByIdAsync(sessionId);
            if (session != null)
            {
                session.MarkAsPaid(transactionId, DateTime.UtcNow);
                await UpdateAsync(session);
            }
        }

        public async Task<IReadOnlyList<Session>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(s => s.User)
                .Include(s => s.Station)
                .Where(s => s.StartTime >= startDate && s.StartTime <= endDate)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<int> GetActiveSessionCountAsync(Guid connectorId)
        {
            return await _dbSet
                .CountAsync(s => s.ConnectorId == connectorId &&
                               (s.Status == ChargeStatus.Charging || s.Status == ChargeStatus.Starting));
        }
    }
}
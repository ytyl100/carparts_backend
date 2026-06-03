// ChargingStationManagement.Infrastructure/Persistence/Repositories/OperatorRepository.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingStationManagement.Infrastructure.Persistence.Repositories
{
    public class OperatorRepository : Repository<Operator>, IRepository<Operator>
    {
        public OperatorRepository(ChargingStationDbContext context) : base(context)
        {
        }

        public override async Task<Operator> GetByIdAsync(string externalId)
        {
            return await _dbSet.FirstOrDefaultAsync(o => o.OperatorId == externalId);
        }

        public async Task<IReadOnlyList<Operator>> GetActiveOperatorsAsync()
        {
            return await _dbSet
                .Include(o => o.Stations)
                .Where(o => o.IsActive)
                .OrderBy(o => o.OperatorName)
                .ToListAsync();
        }

        public async Task<Operator> GetOperatorWithStationsAsync(string operatorId)
        {
            return await _dbSet
                .Include(o => o.Stations)
                    .ThenInclude(s => s.Equipment)
                .FirstOrDefaultAsync(o => o.OperatorId == operatorId);
        }

        public async Task UpdateOperatorApiCredentialsAsync(Guid operatorId, string apiToken, string apiSecret, string encryptionKey = null)
        {
            var op = await GetByIdAsync(operatorId);
            if (op != null)
            {
                op.UpdateApiCredentials(apiToken, apiSecret, encryptionKey);
                await UpdateAsync(op);
            }
        }

        public async Task ActivateOperatorAsync(Guid operatorId)
        {
            var op = await GetByIdAsync(operatorId);
            if (op != null)
            {
                op.Activate();
                await UpdateAsync(op);
            }
        }

        public async Task DeactivateOperatorAsync(Guid operatorId, string reason = null)
        {
            var op = await GetByIdAsync(operatorId);
            if (op != null)
            {
                op.Deactivate(reason);
                await UpdateAsync(op);
            }
        }
    }
}
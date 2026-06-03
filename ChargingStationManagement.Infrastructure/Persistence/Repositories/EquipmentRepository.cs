// ChargingStationManagement.Infrastructure/Persistence/Repositories/EquipmentRepository.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Interfaces;
using ChargingStationManagement.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace ChargingStationManagement.Infrastructure.Persistence.Repositories
{
    public class EquipmentRepository : Repository<Equipment>, IRepository<Equipment>
    {
        public EquipmentRepository(ChargingStationDbContext context) : base(context)
        {
        }

        public override async Task<Equipment> GetByIdAsync(string externalId)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.EquipmentId == externalId);
        }

        public async Task<IReadOnlyList<Equipment>> GetAvailableEquipmentAsync()
        {
            return await _dbSet
                .Include(e => e.Connectors)
                .Where(e => e.Status == EquipmentStatus.Idle &&
                           e.Connectors.Any(c => c.Status == ConnectorStatus.Idle))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Equipment>> GetEquipmentByStationAsync(Guid stationId)
        {
            return await _dbSet
                .Include(e => e.Connectors)
                .Where(e => e.StationId == stationId)
                .OrderBy(e => e.EquipmentName)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Equipment>> GetEquipmentByTypeAsync(EquipmentType equipmentType, bool availableOnly = true)
        {
            var query = _dbSet
                .Include(e => e.Connectors)
                .Where(e => e.EquipmentType == equipmentType);

            if (availableOnly)
            {
                query = query.Where(e => e.Status == EquipmentStatus.Idle &&
                                        e.Connectors.Any(c => c.Status == ConnectorStatus.Idle));
            }

            return await query.ToListAsync();
        }

        public async Task<Equipment> GetEquipmentWithConnectorsAsync(string equipmentId)
        {
            return await _dbSet
                .Include(e => e.Connectors)
                .Include(e => e.Station)
                .FirstOrDefaultAsync(e => e.EquipmentId == equipmentId);
        }

        public async Task UpdateEquipmentStatusAsync(Guid equipmentId, EquipmentStatus status, string reason = null)
        {
            var equipment = await GetByIdAsync(equipmentId);
            if (equipment != null)
            {
                equipment.UpdateStatus(status, reason);
                await UpdateAsync(equipment);
            }
        }

        public async Task UpdateEquipmentPowerAsync(Guid equipmentId, decimal power)
        {
            var equipment = await GetByIdAsync(equipmentId);
            if (equipment != null)
            {
                equipment.SetPower(power);
                await UpdateAsync(equipment);
            }
        }

        public async Task<IReadOnlyList<Equipment>> GetFaultyEquipmentAsync()
        {
            return await _dbSet
                .Include(e => e.Station)
                .Where(e => e.Status == EquipmentStatus.Fault || e.Status == EquipmentStatus.Offline)
                .OrderBy(e => e.EquipmentName)
                .ToListAsync();
        }
    }
}
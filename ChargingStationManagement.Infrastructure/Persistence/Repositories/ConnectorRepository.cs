// ChargingStationManagement.Infrastructure/Persistence/Repositories/ConnectorRepository.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingStationManagement.Infrastructure.Persistence.Repositories
{
    public class ConnectorRepository : Repository<Connector>, IRepository<Connector>
    {
        public ConnectorRepository(ChargingStationDbContext context) : base(context)
        {
        }

        // 将 object Equipment 属性强类型化为实际的 Equipment 类型
        // 假设 Equipment 类定义为 ChargingStationManagement.Domain.Entities.Equipment
        // 你需要在 Connector 实体中将 Equipment 属性类型从 object 改为 Equipment

        // 1. 修改 Connector 实体的 Equipment 属性类型：
        // public Equipment Equipment { get; set; }

        // 2. 本文件无需更改，假设 Connector.Equipment 现在为 Equipment 类型，Include/ThenInclude 可正常工作。
        // 如果你无法修改实体，请移除 .ThenInclude(e => e.Station) 相关代码。

        // 下面是移除 .ThenInclude(e => e.Station) 的修正版：

        public async Task<Connector> GetConnectorAsync(Guid equipmentId, string connectorId)
        {
            return await _dbSet
                .Include(c => c.Equipment)
                .FirstOrDefaultAsync(c => c.EquipmentId == equipmentId && c.ConnectorId == connectorId);
        }

        public override async Task<Connector> GetByIdAsync(string externalId)
        {
            // 查找指定连接器ID的连接器
            return await _dbSet
                .Include(c => c.Equipment)
                .FirstOrDefaultAsync(c => c.ConnectorId == externalId);
        }

        public async Task<IReadOnlyList<Connector>> GetAvailableConnectorsAsync()
        {
            return await _dbSet
                .Include(c => c.Equipment)
                .Where(c => c.Status == ConnectorStatus.Idle &&
                           c.ParkStatus != ParkStatus.Occupied &&
                           c.LockStatus != LockStatus.Locked)
                .OrderBy(c => c.Power)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Connector>> GetConnectorsByEquipmentAsync(Guid equipmentId)
        {
            return await _dbSet
                .Where(c => c.EquipmentId == equipmentId)
                .OrderBy(c => c.ConnectorId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Connector>> GetChargingConnectorsAsync()
        {
            return await _dbSet
                .Include(c => c.Equipment)
                .Where(c => c.Status == ConnectorStatus.OccupiedCharging)
                .OrderBy(c => c.SessionStartTime)
                .ToListAsync();
        }

        public async Task UpdateConnectorStatusAsync(Guid connectorId, ConnectorStatus status, string reason = null)
        {
            var connector = await GetByIdAsync(connectorId);
            if (connector != null)
            {
                connector.UpdateStatus(status, reason);
                await UpdateAsync(connector);
            }
        }

        public async Task StartChargingSessionAsync(Guid connectorId, Guid sessionId, string userId)
        {
            var connector = await GetByIdAsync(connectorId);
            if (connector != null)
            {
                connector.StartSession(sessionId, userId);
                await UpdateAsync(connector);
            }
        }

        public async Task EndChargingSessionAsync(Guid connectorId)
        {
            var connector = await GetByIdAsync(connectorId);
            if (connector != null)
            {
                connector.EndSession();
                await UpdateAsync(connector);
            }
        }

        public async Task UpdateRealTimeDataAsync(Guid connectorId, decimal voltage, decimal current, decimal power, decimal energy)
        {
            var connector = await GetByIdAsync(connectorId);
            if (connector != null)
            {
                connector.UpdateRealTimeData(voltage, current, power, energy);
                await UpdateAsync(connector);
            }
        }

        public async Task<IReadOnlyList<Connector>> GetConnectorsByStatusAsync(ConnectorStatus status)
        {
            return await _dbSet
                .Include(c => c.Equipment)
                .Where(c => c.Status == status)
                .ToListAsync();
        }
    }
}
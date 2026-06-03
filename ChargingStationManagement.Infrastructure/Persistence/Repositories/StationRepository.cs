// ChargingStationManagement.Infrastructure/Persistence/Repositories/StationRepository.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Interfaces;
using ChargingStationManagement.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace ChargingStationManagement.Infrastructure.Persistence.Repositories
{
    public class StationRepository : Repository<Station>, IRepository<Station>
    {
        public StationRepository(ChargingStationDbContext context) : base(context)
        {
        }

        public override async Task<Station> GetByIdAsync(string externalId)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.StationId == externalId);
        }

        public async Task<IReadOnlyList<Station>> GetAvailableStationsAsync(
            decimal userLat,
            decimal userLng,
            decimal radiusKm,
            decimal? minPower = null,
            EquipmentType? equipmentType = null)
        {
            // 简单的距离计算（实际应用中应使用空间数据库查询）
            var latInRadians = (double)userLat * Math.PI / 180;
            var lngDegreeDistance = 111.0 * Math.Cos(latInRadians);
            var minLat = userLat - (radiusKm / 111m);
            var maxLat = userLat + (radiusKm / 111m);
            var minLng = userLng - (radiusKm / (decimal)lngDegreeDistance);
            var maxLng = userLng + (radiusKm / (decimal)lngDegreeDistance);
            var query = _dbSet
                .Include(s => s.Equipment)
                    .ThenInclude(e => e.Connectors)
                .Where(s => s.Status == StationStatus.Normal &&
                           s.StationLat >= minLat && s.StationLat <= maxLat &&
                           s.StationLng >= minLng && s.StationLng <= maxLng &&
                           s.Equipment.Any(e => e.Status == EquipmentStatus.Idle &&
                                                e.Connectors.Any(c => c.Status == ConnectorStatus.Idle)));

            if (minPower.HasValue)
            {
                query = query.Where(s => s.Equipment.Any(e => e.Power >= minPower.Value));
            }

            if (equipmentType.HasValue)
            {
                query = query.Where(s => s.Equipment.Any(e => e.EquipmentType == equipmentType.Value));
            }

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<Station>> GetStationsByOperatorAsync(string operatorId)
        {
            return await _dbSet
                .Include(s => s.Equipment)
                    .ThenInclude(e => e.Connectors)
                .Where(s => s.OperatorId == operatorId && s.Status != StationStatus.Offline)
                .OrderBy(s => s.StationName)
                .ToListAsync();
        }

        public async Task<Station> GetStationWithEquipmentAsync(string stationId)
        {
            return await _dbSet
                .Include(s => s.Equipment)
                    .ThenInclude(e => e.Connectors)
                .Include(s => s.Operator)
                .FirstOrDefaultAsync(s => s.StationId == stationId);
        }

        public async Task UpdateStationStatisticsAsync(Guid stationId, decimal electricity, decimal revenue)
        {
            var station = await GetByIdAsync(stationId);
            if (station != null)
            {
                station.UpdateStatistics(electricity, revenue);
                await UpdateAsync(station);
            }
        }

        public async Task<int> GetAvailableConnectorCountAsync(string stationId)
        {
            return await _context.Connectors
                .Where(c => c.Status == ConnectorStatus.Idle
                    && c.Equipment != null
                    && ((Equipment)c.Equipment).Station != null
                    && ((Station)((Equipment)c.Equipment).Station).StationId == stationId)
                .CountAsync();
        }

        public async Task<IReadOnlyList<Station>> GetStationsWithSpecificationAsync(ISpecification<Station> specification)
        {
            return await GetAsync(specification);
        }
    }
}
// ChargingStationManagement.Services/ApplicationServices/StationManagementService.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Interfaces;
using ChargingStationManagement.Domain.ValueObjects;
using ChargingStationManagement.Services.DTOs;
using ChargingStationManagement.Services.DTOs.ThirdParty;
using ChargingStationManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore; // 🔥 添加这个 using
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ChargingStationManagement.Services.ApplicationServices
{
    public class StationManagementService : IStationManagementService
    {
        private readonly IRepository<Station> _stationRepository;
        private readonly IRepository<Operator> _operatorRepository;
        private readonly IRepository<Equipment> _equipmentRepository;
        private readonly IRepository<Connector> _connectorRepository;
        private readonly IApiThirdPartyIntegrationService _thirdPartyService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<StationManagementService> _logger;

        public StationManagementService(
            IRepository<Station> stationRepository,
            IRepository<Operator> operatorRepository,
            IRepository<Equipment> equipmentRepository,
            IRepository<Connector> connectorRepository,
            IApiThirdPartyIntegrationService thirdPartyService, // 🔥 修复：移除可空标记
            ICacheService cacheService,
            ILogger<StationManagementService> logger)
        {
            _stationRepository = stationRepository;
            _operatorRepository = operatorRepository;
            _equipmentRepository = equipmentRepository;
            _connectorRepository = connectorRepository;
            _thirdPartyService = thirdPartyService; // 🔥 修复：直接赋值
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<List<StationDto>> GetAvailableStationsAsync(decimal latitude, decimal longitude, decimal radiusKm)
        {
            var cacheKey = $"stations_{latitude}_{longitude}_{radiusKm}";

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                try
                {
                    var stations = await _stationRepository.GetAllAsync();

                    // 过滤可用站点
                    var availableStations = new List<Station>();
                    foreach (var station in stations)
                    {
                        // 计算距离（简化实现）
                        var distance = CalculateDistance(
                            latitude, longitude,
                            (double)station.Location.Latitude, (double)station.Location.Longitude);

                        if (distance > (double)radiusKm || station.Status != StationStatus.Normal)
                        {
                            continue;
                        }
                        availableStations.Add(station);
                    }

                    // 转换为DTO
                    var result = new List<StationDto>();
                    foreach (var station in availableStations)
                    {
                        // 🔥 修复：将 string 转换为 Guid
                        Guid operatorGuid = Guid.TryParse(station.OperatorId, out var parsedGuid) 
                            ? parsedGuid 
                            : Guid.Empty;
                        
                        var operatorEntity = await _operatorRepository.GetByIdAsync(operatorGuid);
                        
                        result.Add(new StationDto
                        {
                            StationId = station.StationId,
                            OperatorId = station.OperatorId,
                            OperatorName = operatorEntity?.OperatorName ?? "未知运营商",
                            StationName = station.StationName,
                            Address = station.Address?.FullAddress ?? "",
                            Latitude = station.Location.Latitude,
                            Longitude = station.Location.Longitude,
                            Status = (int)station.Status,
                            StatusText = GetStationStatusText((int)station.Status),
                            AvailableConnectors = station.AvailableConnectors,
                            TotalConnectors = station.TotalConnectors,
                            TotalPower = station.TotalPower,
                            Rates = new RateDto
                            {
                                ElectricityRate = station.ElectricityRate.ElectricityRate,
                                ServiceRate = station.ServiceRate.ServiceRate,
                                ParkRate = station.ParkRate.ParkRate,
                                TimeRate = station.ElectricityRate.TimeRate
                            },
                            Equipment = await GetEquipmentForStationAsync(station.Id),
                            LastUpdated = station.UpdatedAt ?? station.CreatedAt,
                            Source = station.Source
                        });
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting available stations");
                    throw;
                }
            }, TimeSpan.FromMinutes(5)); // 缓存5分钟
        }

        private double CalculateDistance(decimal latitude1, decimal longitude1, double latitude2, double longitude2)
        {
            return CalculateDistance((double)latitude1, (double)longitude1, latitude2, longitude2);
        }

        public async Task<StationDetailDto?> GetStationDetailAsync(string stationId)
        {
            var cacheKey = $"station_detail_{stationId}";

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                try
                {
                    // 🔥 修复：使用 Query().FirstOrDefaultAsync()
                    var station = await _stationRepository.Query()
                        .FirstOrDefaultAsync(s => s.StationId == stationId);

                    if (station == null)
                        return null;

                    // 🔥 修复：将 string 转换为 Guid
                    Guid operatorGuid = Guid.TryParse(station.OperatorId, out var parsedGuid) 
                        ? parsedGuid 
                        : Guid.Empty;
                    
                    var operatorEntity = await _operatorRepository.GetByIdAsync(operatorGuid);

                    var detail = new StationDetailDto
                    {
                        StationId = station.StationId,
                        OperatorId = station.OperatorId,
                        OperatorName = operatorEntity?.OperatorName ?? "未知运营商",
                        StationName = station.StationName,
                        Address = station.Address?.FullAddress ?? "",
                        Latitude = station.Location.Latitude,
                        Longitude = station.Location.Longitude,
                        Status = (int)station.Status,
                        StatusText = GetStationStatusText((int)station.Status),
                        AvailableConnectors = station.AvailableConnectors,
                        TotalConnectors = station.TotalConnectors,
                        TotalPower = station.TotalPower,
                        Rates = new RateDto
                        {
                            ElectricityRate = station.ElectricityRate.ElectricityRate,
                            ServiceRate = station.ServiceRate.ServiceRate,
                            ParkRate = station.ParkRate.ParkRate,
                            TimeRate = station.ElectricityRate.TimeRate
                        },
                        Equipment = await GetEquipmentForStationAsync(station.Id),
                        LastUpdated = station.UpdatedAt ?? station.CreatedAt,
                        Source = station.Source,
                        StationTel = station.StationTel,
                        ServiceTel = station.ServiceTel,
                        SiteGuide = station.SiteGuide,
                        Pictures = station.Pictures?.Split(';').ToList() ?? new List<string>(),
                        BusinessHours = station.BusinessHours,
                        ParkInfo = station.ParkInfo,
                        StationElectricity = station.StationElectricity,
                        TotalRevenue = station.TotalRevenue,
                        StatusHistory = station.StatusHistory?.Select(h => new StationStatusHistoryDto
                        {
                            Status = (int)h.Status,
                            StatusText = GetStationStatusText((int)h.Status),
                            Reason = h.Reason,
                            ChangeTime = h.ChangeTime
                        }).ToList()
                    };

                    return detail;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting station detail for {StationId}", stationId);
                    throw;
                }
            }, TimeSpan.FromMinutes(10)); // 缓存10分钟
        }

        public async Task<List<EquipmentDto>> GetStationEquipmentAsync(string stationId)
        {
            var cacheKey = $"station_equipment_{stationId}";

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                try
                {
                    // 🔥 修复：使用 Query().FirstOrDefaultAsync()
                    var station = await _stationRepository.Query()
                        .FirstOrDefaultAsync(s => s.StationId == stationId);

                    if (station == null)
                        return new List<EquipmentDto>();

                    return await GetEquipmentForStationAsync(station.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting station equipment for {StationId}", stationId);
                    throw;
                }
            }, TimeSpan.FromMinutes(5));
        }

        public async Task<List<ConnectorDto>> GetAvailableConnectorsAsync(string stationId)
        {
            try
            {
                // 🔥 修复：使用 Query().FirstOrDefaultAsync()
                var station = await _stationRepository.Query()
                    .FirstOrDefaultAsync(s => s.StationId == stationId);

                if (station == null)
                    return new List<ConnectorDto>();

                var connectors = new List<ConnectorDto>();
                foreach (var equipment in station.Equipment)
                {
                    foreach (var connector in equipment.Connectors)
                    {
                        if (connector.Status == ConnectorStatus.Idle &&
                            connector.ParkStatus != ParkStatus.Occupied &&
                            connector.LockStatus != LockStatus.Locked)
                        {
                            connectors.Add(new ConnectorDto
                            {
                                ConnectorId = connector.ConnectorId,
                                EquipmentId = equipment.EquipmentId,
                                ConnectorName = connector.ConnectorName,
                                Standard = (int)connector.Standard,
                                StandardText = GetConnectorStandardText((int)connector.Standard),
                                Power = connector.Power,
                                Status = (int)connector.Status,
                                StatusText = GetConnectorStatusText((int)connector.Status),
                                ParkStatus = (int)connector.ParkStatus,
                                ParkStatusText = GetParkStatusText((int)connector.ParkStatus),
                                LockStatus = (int)connector.LockStatus,
                                LockStatusText = GetLockStatusText((int)connector.LockStatus),
                                ParkNo = connector.ParkNo ?? string.Empty, // 🔥 修复：添加 null 合并
                                StatusUpdateTime = connector.StatusUpdateTime,
                                CanStartCharging = connector.CanStartCharging(),
                                VoltageUpperLimits = connector.VoltageUpperLimits,
                                VoltageLowerLimits = connector.VoltageLowerLimits,
                                Current = connector.Current
                            });
                        }
                    }
                }

                return connectors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available connectors for {StationId}", stationId);
                throw;
            }
        }

        public async Task UpdateConnectorStatusAsync(string connectorId, int status, int? parkStatus, int? lockStatus)
        {
            try
            {
                // 🔥 修复：使用 Query().FirstOrDefaultAsync()
                var connector = await _connectorRepository.Query()
                    .FirstOrDefaultAsync(c => c.ConnectorId == connectorId);

                if (connector == null)
                    throw new ArgumentException($"Connector {connectorId} not found");

                // 更新状态
                connector.UpdateStatus((ConnectorStatus)status, "Status pushed from third party");

                if (parkStatus.HasValue)
                {
                    connector.UpdateParkStatus((ParkStatus)parkStatus.Value);
                }

                if (lockStatus.HasValue)
                {
                    connector.UpdateLockStatus((LockStatus)lockStatus.Value);
                }

                await _connectorRepository.UpdateAsync(connector);

                // 清除相关缓存
                await ClearStationCache(connector.EquipmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating connector status for {ConnectorId}", connectorId);
                throw;
            }
        }

        public async Task<List<ConnectorStatusDto>> GetConnectorsStatusAsync(List<string> connectorIds)
        {
            try
            {
                var statusList = new List<ConnectorStatusDto>();

                foreach (var connectorId in connectorIds)
                {
                    // 🔥 修复：使用 Query().FirstOrDefaultAsync()
                    var connector = await _connectorRepository.Query()
                        .FirstOrDefaultAsync(c => c.ConnectorId == connectorId);

                    if (connector != null)
                    {
                        statusList.Add(new ConnectorStatusDto
                        {
                            ConnectorId = connector.ConnectorId,
                            Status = (int)connector.Status,
                            ParkStatus = (int)connector.ParkStatus,
                            LockStatus = (int)connector.LockStatus,
                            UpdateTime = connector.StatusUpdateTime
                        });
                    }
                }

                return statusList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connectors status");
                throw;
            }
        }

        public async Task<ChargingCostDto> CalculateChargingCostAsync(string stationId, decimal energyKwh, TimeSpan duration, bool includeParking)
        {
            try
            {
                // 🔥 修复：使用 Query().FirstOrDefaultAsync()
                var station = await _stationRepository.Query()
                    .FirstOrDefaultAsync(s => s.StationId == stationId);

                if (station == null)
                    throw new ArgumentException($"Station {stationId} not found");

                var cost = station.CalculateChargingCost(energyKwh, duration, includeParking);

                return new ChargingCostDto
                {
                    ElectricityCost = energyKwh * station.ElectricityRate.ElectricityRate,
                    ServiceCost = energyKwh * station.ServiceRate.ServiceRate,
                    ParkCost = includeParking ? (decimal)duration.TotalHours * station.ParkRate.ParkRate : 0,
                    TotalCost = cost,
                    Rates = new RateDto
                    {
                        ElectricityRate = station.ElectricityRate.ElectricityRate,
                        ServiceRate = station.ServiceRate.ServiceRate,
                        ParkRate = station.ParkRate.ParkRate,
                        TimeRate = station.ElectricityRate.TimeRate
                    },
                    EstimatedEnergy = energyKwh,
                    EstimatedDuration = duration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating charging cost for {StationId}", stationId);
                throw;
            }
        }

        public async Task SyncThirdPartyDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting third party data sync");

                // 获取所有配置的第三方
                var thirdParties = new[] { "GuangQi", "XiaoJu", "ThirdParty3" };

                foreach (var thirdParty in thirdParties)
                {
                    try
                    {
                        await SyncThirdPartyData(thirdParty);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing data from {ThirdParty}", thirdParty);
                    }
                }

                // 合并数据
                await MergeMultipleThirdPartyDataAsync(thirdParties.ToList());

                _logger.LogInformation("Third party data sync completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SyncThirdPartyDataAsync");
                throw;
            }
        }

        public async Task MergeMultipleThirdPartyDataAsync(List<string> thirdPartyNames)
        {
            try
            {
                _logger.LogInformation("Starting data merge for {ThirdParties}", string.Join(", ", thirdPartyNames));

                var allStations = new Dictionary<string, Domain.Entities.Station>();

                foreach (var thirdParty in thirdPartyNames)
                {
                    var stations = await _thirdPartyService.SyncStationsAsync(thirdParty);

                    foreach (var stationDto in stations)
                    {
                        var stationKey = $"{stationDto.OperatorID}_{stationDto.StationID}";

                        if (!allStations.ContainsKey(stationKey))
                        {
                            // 创建新站点
                            var station = await CreateOrUpdateStationFromDto(stationDto);
                            allStations[stationKey] = station;
                        }
                        else
                        {
                            // 更新现有站点
                            await UpdateStationFromDto(allStations[stationKey], stationDto);
                        }
                    }
                }

                // 清除所有相关缓存
                await _cacheService.RemoveByPatternAsync("stations_*");
                await _cacheService.RemoveByPatternAsync("station_*");

                _logger.LogInformation("Data merge completed. Total stations: {Count}", allStations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MergeMultipleThirdPartyDataAsync");
                throw;
            }
        }

        private async Task SyncThirdPartyData(string thirdPartyName)
        {
            var stations = await _thirdPartyService.SyncStationsAsync(thirdPartyName);

            foreach (var stationDto in stations)
            {
                await CreateOrUpdateStationFromDto(stationDto);
            }
        }

        private async Task<Station> CreateOrUpdateStationFromDto(ThirdPartyStationDto dto)
        {
            // 🔥 修复：使用 Query().FirstOrDefaultAsync()
            var existingStation = await _stationRepository.Query()
                .FirstOrDefaultAsync(s => s.StationId == dto.StationID && s.OperatorId == dto.OperatorID);

            if (existingStation == null)
            {
                // 创建新站点
                var address = new Address(dto.Address);
                var location = new Coordinates(dto.StationLat, dto.StationLng);

                var station = new Station(
                    dto.StationID,
                    dto.OperatorID,
                    dto.StationName,
                    address,
                    location,
                    dto.Source,
                    "system");

                // 设置其他属性
                station.UpdateBasicInfo(
                    dto.StationName,
                    address,
                    dto.StationTel,
                    dto.ServiceTel,
                    dto.SiteGuide,
                    dto.BusineHours);

                station.UpdateStatus((StationStatus)dto.StationStatus, "Synced from third party");

                // 设置费率
                if (TryParseRate(dto.ElectricityFee, out var electricityRate) &&
                    TryParseRate(dto.ServiceFee, out var serviceRate) &&
                    TryParseRate(dto.ParkFee, out var parkRate))
                {
                    station.UpdateRates(electricityRate, serviceRate, parkRate);
                }

                // 添加设备
                if (dto.EquipmentInfos != null)
                {
                    foreach (var equipmentDto in dto.EquipmentInfos)
                    {
                        await AddEquipmentToStation(station, equipmentDto);
                    }
                }

                await _stationRepository.AddAsync(station);
                return station;
            }
            else
            {
                // 更新现有站点
                await UpdateStationFromDto(existingStation, dto);
                return existingStation;
            }
        }

        private async Task UpdateStationFromDto(Station station, ThirdPartyStationDto dto)
        {
            // 更新基本信息
            var address = new Address(dto.Address);
            station.UpdateBasicInfo(
                dto.StationName,
                address,
                dto.StationTel,
                dto.ServiceTel,
                dto.SiteGuide,
                dto.BusineHours);

            // 更新状态
            if ((int)station.Status != dto.StationStatus)
            {
                station.UpdateStatus((StationStatus)dto.StationStatus, "Updated from third party");
            }

            // 更新费率
            if (TryParseRate(dto.ElectricityFee, out var electricityRate) &&
                TryParseRate(dto.ServiceFee, out var serviceRate) &&
                TryParseRate(dto.ParkFee, out var parkRate))
            {
                station.UpdateRates(electricityRate, serviceRate, parkRate);
            }

            // 更新设备
            if (dto.EquipmentInfos != null)
            {
                // 同步设备
                foreach (var equipmentDto in dto.EquipmentInfos)
                {
                    var existingEquipment = station.GetEquipment(equipmentDto.EquipmentID);

                    if (existingEquipment == null)
                    {
                        await AddEquipmentToStation(station, equipmentDto);
                    }
                    else
                    {
                        await UpdateEquipmentFromDto(existingEquipment, equipmentDto);
                    }
                }
            }

            station.SyncCompleted(dto.Source);
            await _stationRepository.UpdateAsync(station);
        }

        private async Task AddEquipmentToStation(Station station, ThirdPartyEquipmentDto dto)
        {
            var equipment = new Equipment(
                dto.EquipmentID,
                station.Id,
                dto.EquipmentName,
                (EquipmentType)dto.EquipmentType,
                dto.Power,
                station.Source);

            // 添加连接器
            if (dto.ConnectorInfos != null)
            {
                foreach (var connectorDto in dto.ConnectorInfos)
                {
                    var connector = new Connector(
                        connectorDto.ConnectorID,
                        equipment.Id,
                        (ConnectorStandard)connectorDto.NationalStandard,
                        connectorDto.Power,
                        connectorDto.ConnectorName,
                        station.Source);

                    connector.SetTechnicalSpecs(
                        connectorDto.VoltageUpperLimits,
                        connectorDto.VoltageLowerLimits,
                        connectorDto.Current,
                        connectorDto.ParkNo);

                    equipment.AddConnector(connector);
                    await _connectorRepository.AddAsync(connector);
                }
            }

            station.AddEquipment(equipment);
            await _equipmentRepository.AddAsync(equipment);
        }

        private async Task UpdateEquipmentFromDto(Equipment equipment, ThirdPartyEquipmentDto dto)
        {
            // 更新设备信息
            equipment.UpdateTechnicalSpecs(
                dto.EquipmentName,
                "", // 制造商名称
                GetDefaultVoltage((EquipmentType)dto.EquipmentType),
                GetDefaultCurrent(dto.Power),
                "Default Protocol",
                "1.0");

            // 同步连接器
            if (dto.ConnectorInfos != null)
            {
                foreach (var connectorDto in dto.ConnectorInfos)
                {
                    var existingConnector = equipment.GetConnector(connectorDto.ConnectorID);

                    if (existingConnector == null)
                    {
                        var connector = new Connector(
                            connectorDto.ConnectorID,
                            equipment.Id,
                            (ConnectorStandard)connectorDto.NationalStandard,
                            connectorDto.Power,
                            connectorDto.ConnectorName,
                            equipment.Source);

                        connector.SetTechnicalSpecs(
                            connectorDto.VoltageUpperLimits,
                            connectorDto.VoltageLowerLimits,
                            connectorDto.Current,
                            connectorDto.ParkNo);

                        equipment.AddConnector(connector);
                        await _connectorRepository.AddAsync(connector);
                    }
                }
            }

            await _equipmentRepository.UpdateAsync(equipment);
        }

        private async Task<List<EquipmentDto>> GetEquipmentForStationAsync(Guid stationId)
        {
            var equipmentList = new List<EquipmentDto>();

            var station = await _stationRepository.GetByIdAsync(stationId);
            if (station == null)
                return equipmentList;

            foreach (var equipment in station.Equipment)
            {
                var connectors = new List<ConnectorDto>();
                foreach (var connector in equipment.Connectors)
                {
                    connectors.Add(new ConnectorDto
                    {
                        ConnectorId = connector.ConnectorId,
                        EquipmentId = equipment.EquipmentId,
                        ConnectorName = connector.ConnectorName,
                        Standard = (int)connector.Standard,
                        StandardText = GetConnectorStandardText((int)connector.Standard),
                        Power = connector.Power,
                        Status = (int)connector.Status,
                        StatusText = GetConnectorStatusText((int)connector.Status),
                        ParkStatus = (int)connector.ParkStatus,
                        ParkStatusText = GetParkStatusText((int)connector.ParkStatus),
                        LockStatus = (int)connector.LockStatus,
                        LockStatusText = GetLockStatusText((int)connector.LockStatus),
                        ParkNo = connector.ParkNo ?? string.Empty, // 🔥 修复：添加 null 合并
                        StatusUpdateTime = connector.StatusUpdateTime,
                        CanStartCharging = connector.CanStartCharging()
                    });
                }

                equipmentList.Add(new EquipmentDto
                {
                    EquipmentId = equipment.EquipmentId,
                    EquipmentName = equipment.EquipmentName,
                    EquipmentType = (int)equipment.EquipmentType,
                    EquipmentTypeText = GetEquipmentTypeText((int)equipment.EquipmentType),
                    Power = equipment.Power,
                    Status = (int)equipment.Status,
                    StatusText = GetEquipmentStatusText((int)equipment.Status),
                    Connectors = connectors,
                    EquipmentElectricity = equipment.EquipmentElectricity,
                    TotalSessions = equipment.TotalSessions,
                    ManufacturerName = equipment.ManufacturerName ?? string.Empty, // 🔥 修复：添加 null 合并
                    FirmwareVersion = equipment.FirmwareVersion ?? string.Empty // 🔥 修复：添加 null 合并
                });
            }

            return equipmentList;
        }

        private async Task ClearStationCache(Guid equipmentId)
        {
            var equipment = await _equipmentRepository.GetByIdAsync(equipmentId);
            if (equipment != null)
            {
                var station = await _stationRepository.GetByIdAsync(equipment.StationId);
                if (station != null)
                {
                    await _cacheService.RemoveAsync($"station_detail_{station.StationId}");
                    await _cacheService.RemoveAsync($"station_equipment_{station.StationId}");
                }
            }
        }

        // 辅助方法
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // 地球半径（公里）
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees) => degrees * Math.PI / 180;

        private bool TryParseRate(string rateString, out Rate? rate)
        {
            rate = null; // 🔥 修复：改为可空类型
            if (string.IsNullOrEmpty(rateString))
                return false;

            try
            {
                // 简单解析费率字符串，例如："1.5元/kWh"
                var parts = rateString.Split('元');
                if (parts.Length > 0 && decimal.TryParse(parts[0], out var value))
                {
                    rate = new Rate(value, 0, 0, 0);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private decimal GetDefaultVoltage(EquipmentType type)
        {
            return type switch
            {
                EquipmentType.TwoWheeler => 220,
                EquipmentType.FourWheeler => 380,
                EquipmentType.FastCharger => 500,
                _ => 220
            };
        }

        private decimal GetDefaultCurrent(decimal power)
        {
            // 简单计算：电流 = 功率 / 电压 (假设220V)
            return power * 1000 / 220;
        }

        // 状态文本转换方法
        private string GetStationStatusText(int status) => status switch
        {
            0 => "未知",
            1 => "建设中",
            5 => "关闭下线",
            6 => "维护中",
            50 => "正常使用",
            _ => "未知状态"
        };

        private string GetConnectorStatusText(int status) => status switch
        {
            0 => "离网",
            1 => "空闲",
            2 => "占用(未充电)",
            3 => "占用(充电中)",
            4 => "占用(预约锁定)",
            255 => "故障",
            _ => "未知状态"
        };

        private string GetParkStatusText(int status) => status switch
        {
            0 => "未知",
            10 => "空闲",
            50 => "占用",
            _ => "未知"
        };

        private string GetLockStatusText(int status) => status switch
        {
            0 => "未知",
            10 => "已解锁",
            50 => "已上锁",
            _ => "未知"
        };

        private string GetEquipmentTypeText(int type) => type switch
        {
            1 => "二轮充电桩",
            2 => "四轮充电桩",
            3 => "快充桩",
            4 => "换电站",
            _ => "未知类型"
        };

        private string GetEquipmentStatusText(int status) => status switch
        {
            0 => "未知",
            1 => "空闲",
            2 => "待机",
            3 => "充电中",
            4 => "故障",
            5 => "离线",
            6 => "维护中",
            7 => "升级中",
            _ => "未知状态"
        };

        private string GetConnectorStandardText(int standard) => standard switch
        {
            1 => "国标",
            2 => "联合充电系统",
            3 => "日本标准",
            4 => "特斯拉专用",
            5 => "交流充电",
            _ => "未知标准"
        };
    }
}
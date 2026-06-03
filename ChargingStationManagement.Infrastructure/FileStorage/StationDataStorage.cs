// ChargingStationManagement.Infrastructure/FileStorage/StationDataStorage.cs
using ChargingStationManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ChargingStationManagement.Infrastructure.FileStorage
{
    public interface IStationDataStorage
    {
        Task SaveStationsAsync(IEnumerable<Station> stations);
        Task<IEnumerable<Station>> LoadStationsAsync();
        Task SaveStationAsync(Station station);
        Task<Station> LoadStationAsync(string stationId);
        Task SaveStationStatusHistoryAsync(string stationId, IEnumerable<StationStatusHistory> history);
        Task<IEnumerable<StationStatusHistory>> LoadStationStatusHistoryAsync(string stationId, DateTime? startDate = null, DateTime? endDate = null);
    }

    public class StationDataStorage : IStationDataStorage
    {
        private readonly IJsonFileStorage _jsonStorage;
        private readonly ILogger<StationDataStorage> _logger;

        public StationDataStorage(
            IJsonFileStorage jsonStorage,
            ILogger<StationDataStorage> logger)
        {
            _jsonStorage = jsonStorage;
            _logger = logger;
        }

        public async Task SaveStationsAsync(IEnumerable<Station> stations)
        {
            try
            {
                var stationList = stations?.ToList() ?? new List<Station>();
                var filePath = $"stations/all_{DateTime.UtcNow:yyyyMMddHHmmss}.json";

                await _jsonStorage.WriteAsync(filePath, stationList);

                // 同时保存到最新文件
                await _jsonStorage.WriteAsync("stations/latest.json", stationList);

                _logger.LogInformation($"保存了 {stationList.Count} 个充电站到文件");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存充电站数据失败");
                throw;
            }
        }

        public async Task<IEnumerable<Station>> LoadStationsAsync()
        {
            try
            {
                // 首先尝试加载最新文件
                if (await _jsonStorage.FileExistsAsync("stations/latest.json"))
                {
                    return await _jsonStorage.ReadAsync<List<Station>>("stations/latest.json")
                        ?? new List<Station>();
                }

                // 如果没有最新文件，查找最新的备份文件
                var files = await _jsonStorage.ListFilesAsync("stations", "all_*.json");
                var latestFile = files.OrderByDescending(f => f).FirstOrDefault();

                if (!string.IsNullOrEmpty(latestFile))
                {
                    return await _jsonStorage.ReadAsync<List<Station>>($"stations/{latestFile}")
                        ?? new List<Station>();
                }

                return new List<Station>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载充电站数据失败");
                return new List<Station>();
            }
        }

        public async Task SaveStationAsync(Station station)
        {
            try
            {
                if (station == null)
                    throw new ArgumentNullException(nameof(station));

                var filePath = $"stations/individual/{station.StationId}.json";
                await _jsonStorage.WriteAsync(filePath, station);

                _logger.LogDebug($"保存充电站到文件: {station.StationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存充电站数据失败: {station?.StationId}");
                throw;
            }
        }

        public async Task<Station> LoadStationAsync(string stationId)
        {
            try
            {
                var filePath = $"stations/individual/{stationId}.json";

                if (await _jsonStorage.FileExistsAsync(filePath))
                {
                    return await _jsonStorage.ReadAsync<Station>(filePath);
                }

                _logger.LogWarning($"充电站文件不存在: {stationId}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"加载充电站数据失败: {stationId}");
                return null;
            }
        }

        public async Task SaveStationStatusHistoryAsync(string stationId, IEnumerable<StationStatusHistory> history)
        {
            try
            {
                var historyList = history?.ToList() ?? new List<StationStatusHistory>();
                var date = DateTime.UtcNow.ToString("yyyyMMdd");
                var filePath = $"history/station/{stationId}/{date}.json";

                await _jsonStorage.WriteAsync(filePath, historyList);

                _logger.LogDebug($"保存充电站状态历史: {stationId}, 记录数: {historyList.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存充电站状态历史失败: {stationId}");
                throw;
            }
        }

        public async Task<IEnumerable<StationStatusHistory>> LoadStationStatusHistoryAsync(
            string stationId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var directoryPath = $"history/station/{stationId}";
                var allHistory = new List<StationStatusHistory>();

                if (!await _jsonStorage.FileExistsAsync(directoryPath))
                {
                    return allHistory;
                }

                var files = await _jsonStorage.ListFilesAsync(directoryPath, "*.json");

                foreach (var file in files.OrderBy(f => f))
                {
                    try
                    {
                        var fileDate = Path.GetFileNameWithoutExtension(file);
                        if (DateTime.TryParseExact(fileDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var date))
                        {
                            // 过滤日期范围
                            if (startDate.HasValue && date.Date < startDate.Value.Date)
                                continue;

                            if (endDate.HasValue && date.Date > endDate.Value.Date)
                                continue;

                            var history = await _jsonStorage.ReadAsync<List<StationStatusHistory>>(
                                Path.Combine(directoryPath, file));

                            if (history != null)
                            {
                                allHistory.AddRange(history);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"加载历史文件失败: {file}");
                    }
                }

                return allHistory.OrderBy(h => h.ChangeTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"加载充电站状态历史失败: {stationId}");
                return new List<StationStatusHistory>();
            }
        }
    }
}
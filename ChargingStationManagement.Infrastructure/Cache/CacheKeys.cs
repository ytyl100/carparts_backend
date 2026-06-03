// ChargingStationManagement.Infrastructure/Cache/CacheKeys.cs
namespace ChargingStationManagement.Infrastructure.Cache
{
    public static class CacheKeys
    {
        // 充电站相关
        public const string StationsAll = "stations:all";
        public static string StationById(string stationId) => $"station:{stationId}";
        public static string StationWithEquipment(string stationId) => $"station:{stationId}:equipment";
        public static string AvailableStations(decimal lat, decimal lng, decimal radius) =>
            $"stations:available:{lat:F6}:{lng:F6}:{radius:F1}";

        // 设备相关
        public static string EquipmentById(string equipmentId) => $"equipment:{equipmentId}";
        public static string EquipmentByStation(string stationId) => $"equipment:station:{stationId}";

        // 连接器相关
        public static string ConnectorById(string connectorId) => $"connector:{connectorId}";
        public static string AvailableConnectors = "connectors:available";

        // 用户相关
        public static string UserById(string userId) => $"user:{userId}";
        public static string UserByPhone(string phone) => $"user:phone:{phone}";

        // 钱包相关
        public static string WalletByUserId(string userId) => $"wallet:user:{userId}";
        public static string WalletBalance(string walletId) => $"wallet:{walletId}:balance";

        // 会话相关
        public static string SessionById(string sessionId) => $"session:{sessionId}";
        public static string ActiveSessions = "sessions:active";
        public static string UserSessions(string userId) => $"sessions:user:{userId}";

        // 运营商相关
        public static string OperatorById(string operatorId) => $"operator:{operatorId}";
        public const string ActiveOperators = "operators:active";

        // 实时数据
        public const string RealTimeStatus = "realtime:status";
        public static string ConnectorRealTimeData(string connectorId) => $"realtime:connector:{connectorId}";

        // 统计缓存
        public static string StationStats(string stationId, string date) => $"stats:station:{stationId}:{date}";
        public static string DailyRevenue(string date) => $"stats:revenue:daily:{date}";

        // 配置缓存
        public const string SystemConfig = "system:config";
        public const string RateConfig = "system:rates";

        // 地理位置缓存
        public static string GeoStations(decimal lat, decimal lng, int radius) =>
            $"geo:stations:{lat:F2}:{lng:F2}:{radius}";
    }
}
// ChargingStationManagement.Infrastructure/External/ThirdPartyApiSettings.cs
namespace ChargingStationManagement.Infrastructure.External
{
    public class ThirdPartyApiSettings
    {
        // 广汽配置
        public string GuangqiApiBaseUrl { get; set; }
        public string GuangqiApiToken { get; set; }
        public string GuangqiOperatorId { get; set; }
        public string GuangqiApiSecret { get; set; }

        // 小橘配置
        public string OrangeApiBaseUrl { get; set; }
        public string OrangeApiToken { get; set; }
        public string OrangeOperatorId { get; set; }
        public string OrangeApiSecret { get; set; }

        // 特斯拉配置
        public string TeslaApiBaseUrl { get; set; }
        public string TeslaApiToken { get; set; }
        public string TeslaOperatorId { get; set; }
        public string TeslaApiSecret { get; set; }

        // 小桔配置
        public string XiaojuApiBaseUrl { get; set; }
        public string XiaojuApiToken { get; set; }
        public string XiaojuOperatorId { get; set; }
        public string XiaojuApiSecret { get; set; }

        // 通用配置
        public int TimeoutSeconds { get; set; } = 30;
        public int RetryCount { get; set; } = 3;
    }
}
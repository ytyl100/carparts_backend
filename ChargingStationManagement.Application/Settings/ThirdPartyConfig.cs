using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargingStationManagement.Services.Settings
{
    public class ThirdPartyConfig
    {
        public string Name { get; set; } = string.Empty;          // e.g., "GuangQi"
        public string OperatorID { get; set; } = string.Empty;
        public string OperatorSecret { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;       // e.g., "v1"
        public string DataSecret { get; set; } = string.Empty;    // AES key (base64)
        public string DataSecretIV { get; set; } = string.Empty;  // AES IV (base64)
        public string SigSecret { get; set; } = string.Empty;     // HMAC secret
    }
}

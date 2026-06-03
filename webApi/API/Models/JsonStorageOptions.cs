namespace CarPartsInventory.API.Models
{
    public class JsonStorageOptions
    {
        // BasePath kept for compatibility, DataPath is the configurable folder used at runtime
        public string BasePath { get; set; } = @"API\Data";
        // DataPath can be set from appsettings.json (e.g. "JsonStorage:DataPath": "Data")
        public string DataPath { get; set; } = "API\\Data";
        public string DataDirectory { get; internal set; }
    }
}
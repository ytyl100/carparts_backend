namespace CarPartsInventory.API.Models
{
    public class VehicleHierarchyItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, RegionData>? Regions { get; set; }
        public Dictionary<string, ModelData>? Models { get; set; }
    }
}
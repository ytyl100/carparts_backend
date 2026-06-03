using System;
using System.Collections.Generic;

namespace CarPartsInventory.API.Models
{
    public class VehicleHierarchy
    {
        public Dictionary<string, BrandHierarchy> Brands { get; set; } = new();
    }

    public class BrandHierarchy
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, RegionData>? Regions { get; set; }
        public Dictionary<string, ModelData>? Models { get; set; } // 用于没有地区分类的品牌
    }

    public class RegionData
    {
        public Dictionary<string, ModelData> Models { get; set; } = new();
    }

    public class ModelData
    {
        public List<ReleaseData>? Releases { get; set; }
        public List<string>? Codes { get; set; } // 用于没有年款分类的车型
    }

    public class ReleaseData
    {
        public string Period { get; set; } = string.Empty;
        public List<string> Codes { get; set; } = new();
    }

    // DTO for query results
    public class VehicleHierarchyDto
    {
        public string BrandId { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public List<RegionDto> Regions { get; set; } = new();
    }

    public class RegionDto
    {
        public string RegionName { get; set; } = string.Empty;
        public List<ModelDto> Models { get; set; } = new();
    }

    public class ModelDto
    {
        public string ModelName { get; set; } = string.Empty;
        public List<ReleaseDto> Releases { get; set; } = new();
        public List<string> Codes { get; set; } = new();
    }

    public class ReleaseDto
    {
        public string Period { get; set; } = string.Empty;
        public List<string> Codes { get; set; } = new();
    }

    // DTO for vehicle code query
    public class VehicleCodeQueryResult
    {
        public string Code { get; set; } = string.Empty;
        public string BrandId { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string? RegionName { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string? Period { get; set; }
    }
}
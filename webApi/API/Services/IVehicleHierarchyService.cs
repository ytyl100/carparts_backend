using CarPartsInventory.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarPartsInventory.API.Services
{
    public interface IVehicleHierarchyService
    {
        Task<VehicleHierarchy> GetAllAsync();
        Task<VehicleHierarchyDto?> GetByBrandIdAsync(string brandId);
        Task<List<VehicleCodeQueryResult>> GetVehiclesByCodeAsync(string code);
        Task<List<string>> GetAllVehicleCodesAsync();
        Task<List<string>> GetVehicleCodesByBrandAsync(string brandId);
        Task<List<string>> GetVehicleCodesByBrandAndRegionAsync(string brandId, string region);
        Task<List<string>> GetVehicleCodesByBrandAndModelAsync(string brandId, string modelName);
        Task<BrandHierarchy?> GetBrandHierarchyAsync(string brandId);
        Task<Dictionary<string, ModelData>?> GetModelsByBrandAsync(string brandId);
        Task<Dictionary<string, ModelData>?> GetModelsByBrandAndRegionAsync(string brandId, string region);
        
        // CRUD operations for brand hierarchy
        Task<BrandHierarchy> CreateBrandAsync(string brandId, BrandHierarchy brandData);
        Task<BrandHierarchy?> UpdateBrandAsync(string brandId, BrandHierarchy brandData);
        Task<bool> DeleteBrandAsync(string brandId);
        
        // Region operations
        Task<RegionData> AddRegionToBrandAsync(string brandId, string regionName, RegionData regionData);
        Task<bool> DeleteRegionFromBrandAsync(string brandId, string regionName);
        
        // Model operations
        Task<ModelData> AddModelAsync(string brandId, string regionName, string modelName, ModelData modelData);
        Task<ModelData> AddModelDirectlyAsync(string brandId, string modelName, ModelData modelData); // For brands without regions
        Task<bool> DeleteModelAsync(string brandId, string regionName, string modelName);
        Task<bool> DeleteModelDirectlyAsync(string brandId, string modelName);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using CarPartsInventory.API.Models;

namespace CarPartsInventory.API.Services
{
    public interface IBrandService
    {
        Task<IEnumerable<Brand>> GetAllBrandsAsync();
        Task<IEnumerable<Brand>> GetHotBrandsAsync();
        Task<Brand?> GetBrandByIdAsync(string id);
        Task<Brand?> GetBrandByNameAsync(string name);
        Task<Brand> CreateBrandAsync(Brand brand);
        Task<Brand?> UpdateBrandAsync(string id, Brand brand);
        Task<bool> DeleteBrandAsync(string id);
        Task<BrandBatchResult> ReplaceAllBrandsAsync(List<Brand> brands);
    }

    public class BrandBatchResult
    {
        public bool Success { get; set; }
        public int TotalCount { get; set; }
        public int AddedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int RemovedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
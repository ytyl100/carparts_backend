using CarPartsInventory.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarPartsInventory.API.Services
{
    public interface IMainCategoryService
    {
        Task<List<MainCategory>> GetAllAsync();
        Task<MainCategory?> GetByIdAsync(string id);
        Task<List<MainCategory>> GetByVehicleCodeAsync(string vehicleCode);
        Task<MainCategory> CreateAsync(CreateMainCategoryRequest request);
        Task<MainCategory?> UpdateAsync(string id, UpdateMainCategoryRequest request);
        Task<bool> DeleteAsync(string id);
        Task<List<MainCategory>> GetDefaultCategoriesAsync();
    }
}
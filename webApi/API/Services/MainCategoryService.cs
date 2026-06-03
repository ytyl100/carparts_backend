using CarPartsInventory.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarPartsInventory.API.Services
{
    public class MainCategoryService : IMainCategoryService
    {
        private readonly IJsonFileService<MainCategory> _jsonFileService;
        private readonly ILogger<MainCategoryService> _logger;

        public MainCategoryService(
            IJsonFileService<MainCategory> jsonFileService,
            ILogger<MainCategoryService> logger)
        {
            _jsonFileService = jsonFileService;
            _logger = logger;
        }

        public async Task<List<MainCategory>> GetAllAsync()
        {
            return await _jsonFileService.GetAllAsync();
        }

        public async Task<MainCategory?> GetByIdAsync(string id)
        {
            return await _jsonFileService.GetByIdAsync(id);
        }

        public async Task<List<MainCategory>> GetByVehicleCodeAsync(string vehicleCode)
        {
            var allCategories = await _jsonFileService.GetAllAsync();
            return allCategories
                .Where(c => c.VehicleCode == vehicleCode || c.VehicleCode == "*")
                .ToList();
        }

        public async Task<MainCategory> CreateAsync(CreateMainCategoryRequest request)
        {
            try
            {
                var newCategory = new MainCategory
                {
                    Id = $"m_{Guid.NewGuid().ToString("N")[..8]}_{request.VehicleCode}",
                    Name = request.Name,
                    Icon = request.Icon,
                    VehicleCode = request.VehicleCode,
                    IsDefault = request.IsDefault,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                var result = await _jsonFileService.CreateAsync(newCategory);
                _logger.LogInformation("Created main category: {Id}", result.Id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating main category");
                throw;
            }
        }

        public async Task<MainCategory?> UpdateAsync(string id, UpdateMainCategoryRequest request)
        {
            try
            {
                var existing = await _jsonFileService.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogWarning("Main category not found: {Id}", id);
                    return null;
                }

                existing.Name = request.Name;
                existing.Icon = request.Icon;
                existing.IsDefault = request.IsDefault;
                existing.LastUpdated = DateTime.UtcNow;

                var result = await _jsonFileService.UpdateAsync(id, existing);
                _logger.LogInformation("Updated main category: {Id}", id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating main category: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var result = await _jsonFileService.DeleteAsync(id);
                if (result)
                {
                    _logger.LogInformation("Deleted main category: {Id}", id);
                }
                else
                {
                    _logger.LogWarning("Main category not found for deletion: {Id}", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting main category: {Id}", id);
                throw;
            }
        }

        public async Task<List<MainCategory>> GetDefaultCategoriesAsync()
        {
            var allCategories = await _jsonFileService.GetAllAsync();
            return allCategories.Where(c => c.IsDefault).ToList();
        }
    }
}
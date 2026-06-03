using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarPartsInventory.API.Models;
using CarPartsInventory.API.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace CarPartsInventory.API.Services
{
    public class SubCategoryService : ISubCategoryService
    {
        private readonly IJsonFileService<SubCategory> _jsonFileService;
        private readonly ILogger<SubCategoryService> _logger;

        public SubCategoryService(
            IJsonFileService<SubCategory> jsonFileService,
            ILogger<SubCategoryService> logger)
        {
            _jsonFileService = jsonFileService;
            _logger = logger;
        }

        public async Task<List<SubCategory>> GetAllAsync()
        {
            return await _jsonFileService.GetAllAsync();
        }

        public async Task<SubCategory?> GetByIdAsync(string id)
        {
            return await _jsonFileService.GetByIdAsync(id);
        }

        public async Task<List<SubCategory>> GetByParentIdAsync(string parentId)
        {
            var allCategories = await _jsonFileService.GetAllAsync();
            return allCategories.Where(c => c.ParentId == parentId).ToList();
        }

        public async Task<SubCategory> CreateAsync(CreateSubCategoryRequest request)
        {
            try
            {
                var newCategory = new SubCategory
                {
                    Id = $"sub_{Guid.NewGuid().ToString("N")[..8]}",
                    Name = request.Name,
                    Code = request.Code,
                    ParentId = request.ParentId,
                    Image = request.Image,
                    IsDefault = request.IsDefault,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                var result = await _jsonFileService.CreateAsync(newCategory);
                _logger.LogInformation("Created sub category: {Id}", result.Id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sub category");
                throw;
            }
        }

        public async Task<SubCategory?> UpdateAsync(string id, UpdateSubCategoryRequest request)
        {
            try
            {
                var existing = await _jsonFileService.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogWarning("Sub category not found: {Id}", id);
                    return null;
                }

                existing.Name = request.Name;
                existing.Code = request.Code;
                existing.ParentId = request.ParentId;
                existing.Image = request.Image;
                existing.IsDefault = request.IsDefault;
                existing.LastUpdated = DateTime.UtcNow;

                var result = await _jsonFileService.UpdateAsync(id, existing);
                _logger.LogInformation("Updated sub category: {Id}", id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sub category: {Id}", id);
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
                    _logger.LogInformation("Deleted sub category: {Id}", id);
                }
                else
                {
                    _logger.LogWarning("Sub category not found for deletion: {Id}", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sub category: {Id}", id);
                throw;
            }
        }

        public async Task<List<SubCategory>> GetDefaultSubCategoriesAsync()
        {
            var allCategories = await _jsonFileService.GetAllAsync();
            return allCategories.Where(c => c.IsDefault).ToList();
        }

        public async Task<SubCategory?> UpdatePartialAsync(string id, SubCategoryUpdateDto dto)
        {
            try
            {
                var existing = await _jsonFileService.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogWarning("Sub category not found: {Id}", id);
                    return null;
                }

                // 只更新提供的字段
                if (!string.IsNullOrEmpty(dto.Name))
                    existing.Name = dto.Name;

                if (!string.IsNullOrEmpty(dto.Image))
                    existing.Image = dto.Image;

                existing.LastUpdated = DateTime.UtcNow;

                var result = await _jsonFileService.UpdateAsync(id, existing);
                _logger.LogInformation("Partially updated sub category: {Id}", id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error partially updating sub category: {Id}", id);
                throw;
            }
        }
    }
}
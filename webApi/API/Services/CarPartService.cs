using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarPartsInventory.API.Models;

namespace CarPartsInventory.API.Services
{
    public class CarPartService : ICarPartService
    {
        private readonly IJsonFileService<Part> _jsonFileService;

        public CarPartService(IJsonFileService<Part> jsonFileService)
        {
            _jsonFileService = jsonFileService;
        }

        public async Task<IEnumerable<Part>> GetAllPartsAsync() =>
            await _jsonFileService.GetAllAsync();

        public async Task<Part?> GetPartByIdAsync(string id) =>
            await _jsonFileService.GetByIdAsync(id);

        public async Task<Part?> GetPartByPartNumberAsync(string oeNumber)
        {
            var parts = await _jsonFileService.GetAllAsync();
            return parts.FirstOrDefault(p => p.OeNumber.Equals(oeNumber, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Part> CreatePartAsync(Part part)
        {
            var existingPart = await GetPartByPartNumberAsync(part.OeNumber);
            if (existingPart != null)
                throw new InvalidOperationException($"Part with OE {part.OeNumber} already exists.");

            part.Id = Guid.NewGuid().ToString();
            part.Date ??= string.Empty;
            part.LastUpdated = DateTime.UtcNow;

            return await _jsonFileService.CreateAsync(part);
        }

        public async Task<Part?> UpdatePartAsync(string id, Part part)
        {
            var existingPart = await GetPartByIdAsync(id);
            if (existingPart == null)
                return null;

            part.Id = id;
            part.LastUpdated = DateTime.UtcNow;
            return await _jsonFileService.UpdateAsync(id, part);
        }

        public async Task<bool> DeletePartAsync(string id) =>
            await _jsonFileService.DeleteAsync(id);

        /// <summary>
        /// 🔍 增强搜索方法：支持多字段搜索
        /// 搜索优先级：
        /// 1. PartsNumber（零部件编码）
        /// 2. OeNumber（OE号）
        /// 3. StandardName（中文产品名称）
        /// 4. OriginalName（英文产品名称）
        /// 5. Brand（品牌）
        /// 6. Model（型号）
        /// 7. CarModel（车型）
        /// 8. SubCategoryId（分类ID）
        /// </summary>
        public async Task<IEnumerable<Part>> SearchPartsAsync(string searchTerm)
        {
            var parts = await _jsonFileService.GetAllAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
                return parts;

            searchTerm = searchTerm.ToLowerInvariant().Trim();
            
            return parts.Where(p =>
                // 🔍 搜索 PartsNumber（零部件编码）
                (!string.IsNullOrEmpty(p.PartsNumber) && 
                 p.PartsNumber.ToLowerInvariant().Contains(searchTerm)) ||
                
                // 🔍 搜索 OeNumber（OE号）
                (!string.IsNullOrEmpty(p.OeNumber) && 
                 p.OeNumber.ToLowerInvariant().Contains(searchTerm)) ||
                
                // 🔍 搜索中文产品名称
                (!string.IsNullOrEmpty(p.StandardName) && 
                 p.StandardName.ToLowerInvariant().Contains(searchTerm)) ||
                
                // 🔍 搜索英文产品名称
                (!string.IsNullOrEmpty(p.OriginalName) && 
                 p.OriginalName.ToLowerInvariant().Contains(searchTerm)) ||
                
                // 🔍 搜索品牌
                (!string.IsNullOrEmpty(p.Brand) && 
                 p.Brand.ToLowerInvariant().Contains(searchTerm)) ||
                
                // 🔍 搜索型号
                (!string.IsNullOrEmpty(p.Model) && 
                 p.Model.ToLowerInvariant().Contains(searchTerm)) ||
                
                // 🔍 搜索车型
                (!string.IsNullOrEmpty(p.CarModel) && 
                 p.CarModel.ToLowerInvariant().Contains(searchTerm)) ||
                
                // 向后兼容
                p.SubCategoryId.ToLowerInvariant().Contains(searchTerm))
            .ToList();
        }

        public async Task<IEnumerable<Part>> GetPartsByCategoryAsync(string subCategoryId)
        {
            var parts = await _jsonFileService.GetAllAsync();
            return parts.Where(p => p.SubCategoryId.Equals(subCategoryId, StringComparison.OrdinalIgnoreCase));
        }

        public Task<IEnumerable<Part>> GetLowStockPartsAsync(int threshold) =>
            Task.FromResult(Enumerable.Empty<Part>());

        public async Task<bool> UpdateStockAsync(string id, int quantityChange)
        {
            var part = await GetPartByIdAsync(id);
            if (part == null)
                return false;

            if (int.TryParse(part.Quantity, out var qty))
            {
                qty += quantityChange;
                if (qty < 0) qty = 0;
                part.Quantity = qty.ToString();
            }

            part.LastUpdated = DateTime.UtcNow;
            await _jsonFileService.UpdateAsync(id, part);
            return true;
        }

        public async Task<List<Part>> BatchUpdatePartsAsync(List<Part> parts)
        {
            var updatedParts = new List<Part>();

            foreach (var part in parts)
            {
                var existingPart = await GetPartByIdAsync(part.Id);

                if (existingPart != null)
                {
                    part.LastUpdated = DateTime.UtcNow;
                    var updated = await _jsonFileService.UpdateAsync(part.Id, part);
                    if (updated != null)
                        updatedParts.Add(updated);
                }
                else
                {
                    part.LastUpdated = DateTime.UtcNow;
                    var created = await _jsonFileService.CreateAsync(part);
                    updatedParts.Add(created);
                }
            }

            return updatedParts;
        }
    }
}
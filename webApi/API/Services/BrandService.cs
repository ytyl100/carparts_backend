using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarPartsInventory.API.Models;

namespace CarPartsInventory.API.Services
{
    public class BrandService : IBrandService
    {
        private readonly IJsonFileService<Brand> _jsonFileService;

        public BrandService(IJsonFileService<Brand> jsonFileService)
        {
            _jsonFileService = jsonFileService;
        }

        public async Task<IEnumerable<Brand>> GetAllBrandsAsync()
        {
            return await _jsonFileService.GetAllAsync();
        }

        public async Task<Brand> GetBrandByIdAsync(string id)
        {
            return await _jsonFileService.GetByIdAsync(id);
        }

        public async Task<Brand> CreateBrandAsync(Brand brand)
        {
            // 检查品牌是否已存在
            var brands = await _jsonFileService.GetAllAsync();
            if (brands.Any(b => b.Name.Equals(brand.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Brand with name {brand.Name} already exists.");
            }

            if (string.IsNullOrEmpty(brand.Id))
            {
                brand.Id = Guid.NewGuid().ToString();
            }

            brand.CreatedDate = DateTime.UtcNow;
            brand.LastUpdated = DateTime.UtcNow;

            return await _jsonFileService.CreateAsync(brand);
        }

        public async Task<Brand> UpdateBrandAsync(string id, Brand brand)
        {
            var existingBrand = await GetBrandByIdAsync(id);
            if (existingBrand == null)
                return null;

            brand.Id = id;
            brand.CreatedDate = existingBrand.CreatedDate;
            brand.LastUpdated = DateTime.UtcNow;

            return await _jsonFileService.UpdateAsync(id, brand);
        }

        public async Task<bool> DeleteBrandAsync(string id)
        {
            return await _jsonFileService.DeleteAsync(id);
        }

        public async Task<IEnumerable<Brand>> GetHotBrandsAsync()
        {
            var brands = await _jsonFileService.GetAllAsync();
            return brands.Where(b => b.IsHot);
        }

        public async Task<Brand> GetBrandByNameAsync(string name)
        {
            var brands = await _jsonFileService.GetAllAsync();
            return brands.FirstOrDefault(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<BrandBatchResult> ReplaceAllBrandsAsync(List<Brand> brands)
        {
            var existingBrands = (await _jsonFileService.GetAllAsync()).ToList();

            var existingIds = existingBrands.Select(b => b.Id).ToHashSet();
            var incomingIds = brands.Select(b => b.Id).ToHashSet();

            var addedCount = incomingIds.Except(existingIds).Count();
            var removedCount = existingIds.Except(incomingIds).Count();
            var updatedCount = incomingIds.Intersect(existingIds).Count();

            var now = DateTime.UtcNow;
            foreach (var brand in brands)
            {
                if (string.IsNullOrEmpty(brand.Id))
                    brand.Id = Guid.NewGuid().ToString();

                // 新增项设置创建时间，已有项保留原创建时间
                var existing = existingBrands.FirstOrDefault(b => b.Id == brand.Id);
                if (existing != null)
                {
                    if (brand.CreatedDate == default)
                        brand.CreatedDate = existing.CreatedDate;
                }
                else
                {
                    if (brand.CreatedDate == default)
                        brand.CreatedDate = now;
                }

                brand.LastUpdated = now;
            }

            await _jsonFileService.ReplaceAllAsync(brands);

            return new BrandBatchResult
            {
                Success = true,
                TotalCount = brands.Count,
                AddedCount = addedCount,
                UpdatedCount = updatedCount,
                RemovedCount = removedCount,
                Message = $"批量更新完成：共 {brands.Count} 条，新增 {addedCount}，更新 {updatedCount}，删除 {removedCount}"
            };
        }
    }
}
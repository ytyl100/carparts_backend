using CarPartsInventory.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarPartsInventory.API.Services
{
    public class VehicleHierarchyService : IVehicleHierarchyService
    {
        private readonly IJsonFileService<VehicleHierarchyItem> _jsonFileService;
        private readonly ILogger<VehicleHierarchyService> _logger;

        public VehicleHierarchyService(
            IJsonFileService<VehicleHierarchyItem> jsonFileService,
            ILogger<VehicleHierarchyService> logger)
        {
            _jsonFileService = jsonFileService;
            _logger = logger;
        }

        // 将列表转换为字典格式供内部使用
        private async Task<VehicleHierarchy> GetHierarchyAsync()
        {
            var items = await _jsonFileService.GetAllAsync();
            
            var brands = items.ToDictionary(
                item => item.Id,
                item => new BrandHierarchy
                {
                    Id = item.Id,
                    Name = item.Name,
                    Regions = item.Regions,
                    Models = item.Models
                }
            );

            return new VehicleHierarchy { Brands = brands };
        }

        // 将字典转换回列表格式保存
        private async Task SaveHierarchyAsync(VehicleHierarchy hierarchy)
        {
            var currentItems = await _jsonFileService.GetAllAsync();
            var brandIds = hierarchy.Brands.Keys.ToHashSet();

            // 更新现有项和添加新项
            foreach (var brand in hierarchy.Brands)
            {
                var existingItem = currentItems.FirstOrDefault(i => i.Id == brand.Key);
                
                var item = new VehicleHierarchyItem
                {
                    Id = brand.Key,
                    Name = brand.Value.Name,
                    Regions = brand.Value.Regions,
                    Models = brand.Value.Models
                };

                if (existingItem != null)
                {
                    await _jsonFileService.UpdateAsync(brand.Key, item);
                }
                else
                {
                    await _jsonFileService.CreateAsync(item);
                }
            }

            // 删除不再存在的项
            var itemsToDelete = currentItems.Where(i => !brandIds.Contains(i.Id)).ToList();
            foreach (var item in itemsToDelete)
            {
                await _jsonFileService.DeleteAsync(item.Id);
            }
        }

        public async Task<VehicleHierarchy> GetAllAsync()
        {
            return await GetHierarchyAsync();
        }

        public async Task<VehicleHierarchyDto?> GetByBrandIdAsync(string brandId)
        {
            var data = await GetHierarchyAsync();

            if (!data.Brands.TryGetValue(brandId, out var brandData))
            {
                _logger.LogWarning("Brand not found: {BrandId}", brandId);
                return null;
            }

            var dto = new VehicleHierarchyDto
            {
                BrandId = brandId,
                BrandName = brandData.Name
            };

            if (brandData.Regions != null)
            {
                foreach (var region in brandData.Regions)
                {
                    var regionDto = new RegionDto
                    {
                        RegionName = region.Key
                    };

                    foreach (var model in region.Value.Models)
                    {
                        var modelDto = new ModelDto
                        {
                            ModelName = model.Key
                        };

                        if (model.Value.Releases != null)
                        {
                            modelDto.Releases = model.Value.Releases.Select(r => new ReleaseDto
                            {
                                Period = r.Period,
                                Codes = r.Codes
                            }).ToList();
                        }
                        else if (model.Value.Codes != null)
                        {
                            modelDto.Codes = model.Value.Codes;
                        }

                        regionDto.Models.Add(modelDto);
                    }

                    dto.Regions.Add(regionDto);
                }
            }
            else if (brandData.Models != null)
            {
                var regionDto = new RegionDto
                {
                    RegionName = "Default"
                };

                foreach (var model in brandData.Models)
                {
                    var modelDto = new ModelDto
                    {
                        ModelName = model.Key
                    };

                    if (model.Value.Releases != null)
                    {
                        modelDto.Releases = model.Value.Releases.Select(r => new ReleaseDto
                        {
                            Period = r.Period,
                            Codes = r.Codes
                        }).ToList();
                    }
                    else if (model.Value.Codes != null)
                    {
                        modelDto.Codes = model.Value.Codes;
                    }

                    regionDto.Models.Add(modelDto);
                }

                dto.Regions.Add(regionDto);
            }

            return dto;
        }

        public async Task<List<VehicleCodeQueryResult>> GetVehiclesByCodeAsync(string code)
        {
            var data = await GetHierarchyAsync();
            var results = new List<VehicleCodeQueryResult>();

            foreach (var brand in data.Brands)
            {
                if (brand.Value.Regions != null)
                {
                    foreach (var region in brand.Value.Regions)
                    {
                        foreach (var model in region.Value.Models)
                        {
                            if (model.Value.Releases != null)
                            {
                                foreach (var release in model.Value.Releases)
                                {
                                    if (release.Codes.Any(c => c.Equals(code, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        results.Add(new VehicleCodeQueryResult
                                        {
                                            Code = code,
                                            BrandId = brand.Key,
                                            BrandName = brand.Value.Name,
                                            RegionName = region.Key,
                                            ModelName = model.Key,
                                            Period = release.Period
                                        });
                                    }
                                }
                            }
                            else if (model.Value.Codes != null &&
                                     model.Value.Codes.Any(c => c.Equals(code, StringComparison.OrdinalIgnoreCase)))
                            {
                                results.Add(new VehicleCodeQueryResult
                                {
                                    Code = code,
                                    BrandId = brand.Key,
                                    BrandName = brand.Value.Name,
                                    RegionName = region.Key,
                                    ModelName = model.Key
                                });
                            }
                        }
                    }
                }
                else if (brand.Value.Models != null)
                {
                    foreach (var model in brand.Value.Models)
                    {
                        if (model.Value.Releases != null)
                        {
                            foreach (var release in model.Value.Releases)
                            {
                                if (release.Codes.Any(c => c.Equals(code, StringComparison.OrdinalIgnoreCase)))
                                {
                                    results.Add(new VehicleCodeQueryResult
                                    {
                                        Code = code,
                                        BrandId = brand.Key,
                                        BrandName = brand.Value.Name,
                                        ModelName = model.Key,
                                        Period = release.Period
                                    });
                                }
                            }
                        }
                        else if (model.Value.Codes != null &&
                                 model.Value.Codes.Any(c => c.Equals(code, StringComparison.OrdinalIgnoreCase)))
                        {
                            results.Add(new VehicleCodeQueryResult
                            {
                                Code = code,
                                BrandId = brand.Key,
                                BrandName = brand.Value.Name,
                                ModelName = model.Key
                            });
                        }
                    }
                }
            }

            _logger.LogInformation("Found {Count} vehicles with code: {Code}", results.Count, code);
            return results;
        }

        public async Task<List<string>> GetAllVehicleCodesAsync()
        {
            var data = await GetHierarchyAsync();
            var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var brand in data.Brands.Values)
            {
                if (brand.Regions != null)
                {
                    foreach (var region in brand.Regions.Values)
                    {
                        foreach (var model in region.Models.Values)
                        {
                            if (model.Releases != null)
                            {
                                foreach (var release in model.Releases)
                                {
                                    codes.UnionWith(release.Codes);
                                }
                            }
                            else if (model.Codes != null)
                            {
                                codes.UnionWith(model.Codes);
                            }
                        }
                    }
                }
                else if (brand.Models != null)
                {
                    foreach (var model in brand.Models.Values)
                    {
                        if (model.Releases != null)
                        {
                            foreach (var release in model.Releases)
                            {
                                codes.UnionWith(release.Codes);
                            }
                        }
                        else if (model.Codes != null)
                        {
                            codes.UnionWith(model.Codes);
                        }
                    }
                }
            }

            return codes.OrderBy(c => c).ToList();
        }

        public async Task<List<string>> GetVehicleCodesByBrandAsync(string brandId)
        {
            var data = await GetHierarchyAsync();
            var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (data.Brands.TryGetValue(brandId, out var brand))
            {
                if (brand.Regions != null)
                {
                    foreach (var region in brand.Regions.Values)
                    {
                        foreach (var model in region.Models.Values)
                        {
                            if (model.Releases != null)
                            {
                                foreach (var release in model.Releases)
                                {
                                    codes.UnionWith(release.Codes);
                                }
                            }
                            else if (model.Codes != null)
                            {
                                codes.UnionWith(model.Codes);
                            }
                        }
                    }
                }
                else if (brand.Models != null)
                {
                    foreach (var model in brand.Models.Values)
                    {
                        if (model.Releases != null)
                        {
                            foreach (var release in model.Releases)
                            {
                                codes.UnionWith(release.Codes);
                            }
                        }
                        else if (model.Codes != null)
                        {
                            codes.UnionWith(model.Codes);
                        }
                    }
                }
            }

            return codes.OrderBy(c => c).ToList();
        }

        public async Task<List<string>> GetVehicleCodesByBrandAndRegionAsync(string brandId, string region)
        {
            var data = await GetHierarchyAsync();
            var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (data.Brands.TryGetValue(brandId, out var brand) &&
                brand.Regions != null &&
                brand.Regions.TryGetValue(region, out var regionData))
            {
                foreach (var model in regionData.Models.Values)
                {
                    if (model.Releases != null)
                    {
                        foreach (var release in model.Releases)
                        {
                            codes.UnionWith(release.Codes);
                        }
                    }
                    else if (model.Codes != null)
                    {
                        codes.UnionWith(model.Codes);
                    }
                }
            }

            return codes.OrderBy(c => c).ToList();
        }

        public async Task<List<string>> GetVehicleCodesByBrandAndModelAsync(string brandId, string modelName)
        {
            var data = await GetHierarchyAsync();
            var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (data.Brands.TryGetValue(brandId, out var brand))
            {
                if (brand.Regions != null)
                {
                    foreach (var region in brand.Regions.Values)
                    {
                        if (region.Models.TryGetValue(modelName, out var model))
                        {
                            if (model.Releases != null)
                            {
                                foreach (var release in model.Releases)
                                {
                                    codes.UnionWith(release.Codes);
                                }
                            }
                            else if (model.Codes != null)
                            {
                                codes.UnionWith(model.Codes);
                            }
                        }
                    }
                }
                else if (brand.Models != null && brand.Models.TryGetValue(modelName, out var model))
                {
                    if (model.Releases != null)
                    {
                        foreach (var release in model.Releases)
                        {
                            codes.UnionWith(release.Codes);
                        }
                    }
                    else if (model.Codes != null)
                    {
                        codes.UnionWith(model.Codes);
                    }
                }
            }

            return codes.OrderBy(c => c).ToList();
        }

        public async Task<BrandHierarchy?> GetBrandHierarchyAsync(string brandId)
        {
            var data = await GetHierarchyAsync();
            return data.Brands.TryGetValue(brandId, out var brand) ? brand : null;
        }

        public async Task<Dictionary<string, ModelData>?> GetModelsByBrandAsync(string brandId)
        {
            var data = await GetHierarchyAsync();
            return data.Brands.TryGetValue(brandId, out var brand) ? brand.Models : null;
        }

        public async Task<Dictionary<string, ModelData>?> GetModelsByBrandAndRegionAsync(string brandId, string region)
        {
            var data = await GetHierarchyAsync();
            if (data.Brands.TryGetValue(brandId, out var brand) &&
                brand.Regions != null &&
                brand.Regions.TryGetValue(region, out var regionData))
            {
                return regionData.Models;
            }
            return null;
        }

        public async Task<BrandHierarchy> CreateBrandAsync(string brandId, BrandHierarchy brandData)
        {
            try
            {
                var data = await GetHierarchyAsync();

                if (data.Brands.ContainsKey(brandId))
                {
                    throw new InvalidOperationException($"Brand with ID {brandId} already exists");
                }

                data.Brands[brandId] = brandData;
                await SaveHierarchyAsync(data);

                _logger.LogInformation("Created brand in hierarchy: {BrandId}", brandId);
                return brandData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand in hierarchy: {BrandId}", brandId);
                throw;
            }
        }

        public async Task<BrandHierarchy?> UpdateBrandAsync(string brandId, BrandHierarchy brandData)
        {
            try
            {
                var data = await GetHierarchyAsync();

                if (!data.Brands.TryGetValue(brandId, out var existingBrand))
                {
                    _logger.LogWarning("Brand not found in hierarchy: {BrandId}", brandId);
                    return null;
                }

                // 保留已有的 Name（前端可能不传）
                if (string.IsNullOrEmpty(brandData.Name))
                {
                    brandData.Name = existingBrand.Name;
                }

                brandData.Id = brandId;
                data.Brands[brandId] = brandData;
                await SaveHierarchyAsync(data);

                _logger.LogInformation("Updated brand in hierarchy: {BrandId}", brandId);
                return brandData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand in hierarchy: {BrandId}", brandId);
                throw;
            }
        }

        public async Task<bool> DeleteBrandAsync(string brandId)
        {
            try
            {
                var data = await GetHierarchyAsync();

                if (!data.Brands.ContainsKey(brandId))
                {
                    _logger.LogWarning("Brand not found for deletion in hierarchy: {BrandId}", brandId);
                    return false;
                }

                data.Brands.Remove(brandId);
                await SaveHierarchyAsync(data);

                _logger.LogInformation("Deleted brand from hierarchy: {BrandId}", brandId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand from hierarchy: {BrandId}", brandId);
                throw;
            }
        }

        public async Task<RegionData> AddRegionToBrandAsync(string brandId, string regionName, RegionData regionData)
        {
            try
            {
                var data = await GetHierarchyAsync();

                if (!data.Brands.TryGetValue(brandId, out var brand))
                {
                    throw new InvalidOperationException($"Brand with ID {brandId} not found");
                }

                brand.Regions ??= new Dictionary<string, RegionData>();

                if (brand.Regions.ContainsKey(regionName))
                {
                    throw new InvalidOperationException($"Region {regionName} already exists for brand {brandId}");
                }

                brand.Regions[regionName] = regionData;
                await SaveHierarchyAsync(data);

                _logger.LogInformation("Added region {RegionName} to brand {BrandId}", regionName, brandId);
                return regionData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding region to brand: {BrandId}/{RegionName}", brandId, regionName);
                throw;
            }
        }

        public async Task<bool> DeleteRegionFromBrandAsync(string brandId, string regionName)
        {
            try
            {
                var data = await GetHierarchyAsync();

                if (!data.Brands.TryGetValue(brandId, out var brand) || brand.Regions == null)
                {
                    _logger.LogWarning("Brand or regions not found: {BrandId}", brandId);
                    return false;
                }

                if (!brand.Regions.Remove(regionName))
                {
                    _logger.LogWarning("Region not found: {BrandId}/{RegionName}", brandId, regionName);
                    return false;
                }

                await SaveHierarchyAsync(data);
                _logger.LogInformation("Deleted region {RegionName} from brand {BrandId}", regionName, brandId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting region from brand: {BrandId}/{RegionName}", brandId, regionName);
                throw;
            }
        }

        public async Task<ModelData> AddModelAsync(string brandId, string regionName, string modelName, ModelData modelData)
        {
            try
            {
                var data = await GetHierarchyAsync();

                if (!data.Brands.TryGetValue(brandId, out var brand) ||
                    brand.Regions == null ||
                    !brand.Regions.TryGetValue(regionName, out var region))
                {
                    throw new InvalidOperationException($"Brand {brandId} or region {regionName} not found");
                }

                if (region.Models.ContainsKey(modelName))
                {
                    throw new InvalidOperationException($"Model {modelName} already exists");
                }

                region.Models[modelName] = modelData;
                await SaveHierarchyAsync(data);

                _logger.LogInformation("Added model {ModelName} to {BrandId}/{RegionName}", modelName, brandId, regionName);
                return modelData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding model: {BrandId}/{RegionName}/{ModelName}", brandId, regionName, modelName);
                throw;
            }
        }

        public async Task<ModelData> AddModelDirectlyAsync(string brandId, string modelName, ModelData modelData)
        {
            try
            {
                var data = await GetHierarchyAsync();

                if (!data.Brands.TryGetValue(brandId, out var brand))
                {
                    throw new InvalidOperationException($"Brand {brandId} not found");
                }

                brand.Models ??= new Dictionary<string, ModelData>();

                if (brand.Models.ContainsKey(modelName))
                {
                    throw new InvalidOperationException($"Model {modelName} already exists");
                }

                brand.Models[modelName] = modelData;
                await SaveHierarchyAsync(data);

                _logger.LogInformation("Added model {ModelName} directly to brand {BrandId}", modelName, brandId);
                return modelData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding model directly: {BrandId}/{ModelName}", brandId, modelName);
                throw;
            }
        }

        public async Task<bool> DeleteModelAsync(string brandId, string regionName, string modelName)
        {
            try
            {
                var data = await GetHierarchyAsync();

                if (!data.Brands.TryGetValue(brandId, out var brand) ||
                    brand.Regions == null ||
                    !brand.Regions.TryGetValue(regionName, out var region))
                {
                    _logger.LogWarning("Brand or region not found: {BrandId}/{RegionName}", brandId, regionName);
                    return false;
                }

                if (!region.Models.Remove(modelName))
                {
                    _logger.LogWarning("Model not found: {BrandId}/{RegionName}/{ModelName}", brandId, regionName, modelName);
                    return false;
                }

                await SaveHierarchyAsync(data);
                _logger.LogInformation("Deleted model {ModelName} from {BrandId}/{RegionName}", modelName, brandId, regionName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting model: {BrandId}/{RegionName}/{ModelName}", brandId, regionName, modelName);
                throw;
            }
        }

        public async Task<bool> DeleteModelDirectlyAsync(string brandId, string modelName)
        {
            try
            {
                var data = await GetHierarchyAsync();

                if (!data.Brands.TryGetValue(brandId, out var brand) || brand.Models == null)
                {
                    _logger.LogWarning("Brand or models not found: {BrandId}", brandId);
                    return false;
                }

                if (!brand.Models.Remove(modelName))
                {
                    _logger.LogWarning("Model not found: {BrandId}/{ModelName}", brandId, modelName);
                    return false;
                }

                await SaveHierarchyAsync(data);
                _logger.LogInformation("Deleted model {ModelName} directly from brand {BrandId}", modelName, brandId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting model directly: {BrandId}/{ModelName}", brandId, modelName);
                throw;
            }
        }
    }
}

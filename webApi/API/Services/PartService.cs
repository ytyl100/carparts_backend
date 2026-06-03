using CarPartsInventory.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarPartsInventory.API.Services
{
    public class PartService : IPartService
    {
        private readonly IJsonFileService<Part> _jsonFileService;
        private readonly ILogger<PartService> _logger;

        public PartService(
            IJsonFileService<Part> jsonFileService,
            ILogger<PartService> logger)
        {
            _jsonFileService = jsonFileService;
            _logger = logger;
        }

        public async Task<List<Part>> GetAllAsync()
        {
            return await _jsonFileService.GetAllAsync();
        }

        public async Task<Part?> GetByIdAsync(string id)
        {
            return await _jsonFileService.GetByIdAsync(id);
        }

        public async Task<List<Part>> GetBySubCategoryIdAsync(string subCategoryId)
        {
            var allParts = await _jsonFileService.GetAllAsync();
            return allParts.Where(p => p.SubCategoryId == subCategoryId).ToList();
        }

        public async Task<List<Part>> GetByOeNumberAsync(string oeNumber)
        {
            var allParts = await _jsonFileService.GetAllAsync();
            return allParts
                .Where(p => p.OeNumber.Contains(oeNumber, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<List<Part>> GetByPositionAsync(string position)
        {
            var allParts = await _jsonFileService.GetAllAsync();
            return allParts
                .Where(p => p.Position.Contains(position, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<List<Part>> SearchAsync(PartSearchRequest request)
        {
            var allParts = await _jsonFileService.GetAllAsync();
            var query = allParts.AsEnumerable();

            if (!string.IsNullOrEmpty(request.SubCategoryId))
            {
                query = query.Where(p => p.SubCategoryId == request.SubCategoryId);
            }

            if (!string.IsNullOrEmpty(request.OeNumber))
            {
                query = query.Where(p => p.OeNumber.Contains(request.OeNumber, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(request.StandardName))
            {
                query = query.Where(p => p.StandardName.Contains(request.StandardName, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(request.Position))
            {
                query = query.Where(p => p.Position.Contains(request.Position, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(request.Brand))
            {
                query = query.Where(p => p.PriceRecords.Any(pr => 
                    pr.Brand.Contains(request.Brand, StringComparison.OrdinalIgnoreCase)));
            }

            if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
            {
                query = query.Where(p => p.PriceRecords.Any(pr =>
                {
                    if (request.MinPrice.HasValue && pr.SaleInclTax < request.MinPrice.Value)
                        return false;
                    if (request.MaxPrice.HasValue && pr.SaleInclTax > request.MaxPrice.Value)
                        return false;
                    return true;
                }));
            }

            if (!string.IsNullOrEmpty(request.ModelCode))
            {
                query = query.Where(p => p.AdaptableModels.Any(am => 
                    am.ModelCode.Contains(request.ModelCode, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrEmpty(request.ReplacementOe))
            {
                query = query.Where(p => p.ReplacementParts.Any(rp => 
                    rp.ReplacementOe.Contains(request.ReplacementOe, StringComparison.OrdinalIgnoreCase) ||
                    rp.OriginalOe.Contains(request.ReplacementOe, StringComparison.OrdinalIgnoreCase)));
            }

            return query.ToList();
        }

        public async Task<Part> CreateAsync(CreatePartRequest request)
        {
            try
            {
                var newPart = new Part
                {
                    Id = $"part_{Guid.NewGuid().ToString("N")[..12]}",
                    SubCategoryId = request.SubCategoryId,
                    Position = request.Position,
                    OeNumber = request.OeNumber,
                    StandardName = request.StandardName,
                    OriginalName = request.OriginalName,
                    Quantity = request.Quantity,
                    Note = request.Note,
                    Date = request.Date,
                    X = request.X,
                    Y = request.Y,
                    ImageUrl = request.ImageUrl,
                    PriceRecords = request.PriceRecords ?? new List<PriceRecord>(),
                    ReplacementParts = request.ReplacementParts ?? new List<ReplacementPart>(),
                    AdaptableModels = request.AdaptableModels ?? new List<AdaptableModel>()
                };

                var result = await _jsonFileService.CreateAsync(newPart);
                _logger.LogInformation("Created part: {Id}", result.Id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating part");
                throw;
            }
        }

        public async Task<Part?> UpdateAsync(string id, UpdatePartRequest request)
        {
            try
            {
                var existing = await _jsonFileService.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogWarning("Part not found: {Id}", id);
                    return null;
                }

                existing.SubCategoryId = request.SubCategoryId;
                existing.Position = request.Position;
                existing.OeNumber = request.OeNumber;
                existing.StandardName = request.StandardName;
                existing.OriginalName = request.OriginalName;
                existing.Quantity = request.Quantity;
                existing.Note = request.Note;
                existing.Date = request.Date;
                existing.X = request.X;
                existing.Y = request.Y;
                existing.ImageUrl = request.ImageUrl;
                existing.PriceRecords = request.PriceRecords ?? new List<PriceRecord>();
                existing.ReplacementParts = request.ReplacementParts ?? new List<ReplacementPart>();
                existing.AdaptableModels = request.AdaptableModels ?? new List<AdaptableModel>();

                var result = await _jsonFileService.UpdateAsync(id, existing);
                _logger.LogInformation("Updated part: {Id}", id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part: {Id}", id);
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
                    _logger.LogInformation("Deleted part: {Id}", id);
                }
                else
                {
                    _logger.LogWarning("Part not found for deletion: {Id}", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting part: {Id}", id);
                throw;
            }
        }

        public async Task<List<Part>> GetByReplacementOeAsync(string oeNumber)
        {
            var allParts = await _jsonFileService.GetAllAsync();
            return allParts
                .Where(p => p.ReplacementParts.Any(rp => 
                    rp.OriginalOe.Contains(oeNumber, StringComparison.OrdinalIgnoreCase) ||
                    rp.ReplacementOe.Contains(oeNumber, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public async Task<List<Part>> GetByModelCodeAsync(string modelCode)
        {
            var allParts = await _jsonFileService.GetAllAsync();
            return allParts
                .Where(p => p.AdaptableModels.Any(am => 
                    am.ModelCode.Contains(modelCode, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public async Task<List<Part>> GetByAdaptableBrandAsync(string brand)
        {
            var allParts = await _jsonFileService.GetAllAsync();
            return allParts
                .Where(p => p.AdaptableModels.Any(am => 
                    am.Brand.Contains(brand, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public async Task<Part?> AddReplacementPartAsync(string partId, ReplacementPart replacementPart)
        {
            try
            {
                var part = await _jsonFileService.GetByIdAsync(partId);
                if (part == null)
                {
                    _logger.LogWarning("Part not found: {Id}", partId);
                    return null;
                }

                part.ReplacementParts.Add(replacementPart);
                var result = await _jsonFileService.UpdateAsync(partId, part);
                
                _logger.LogInformation("Added replacement part to: {Id}", partId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding replacement part to: {Id}", partId);
                throw;
            }
        }

        public async Task<Part?> AddAdaptableModelAsync(string partId, AdaptableModel adaptableModel)
        {
            try
            {
                var part = await _jsonFileService.GetByIdAsync(partId);
                if (part == null)
                {
                    _logger.LogWarning("Part not found: {Id}", partId);
                    return null;
                }

                part.AdaptableModels.Add(adaptableModel);
                var result = await _jsonFileService.UpdateAsync(partId, part);
                
                _logger.LogInformation("Added adaptable model to: {Id}", partId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding adaptable model to: {Id}", partId);
                throw;
            }
        }

        public async Task<Part?> UpdateReplacementPartAsync(string partId, string replacementOe, ReplacementPart updatedReplacementPart)
        {
            try
            {
                var part = await _jsonFileService.GetByIdAsync(partId);
                if (part == null)
                {
                    _logger.LogWarning("Part not found: {Id}", partId);
                    return null;
                }

                var index = part.ReplacementParts.FindIndex(rp => 
                    rp.ReplacementOe.Equals(replacementOe, StringComparison.OrdinalIgnoreCase));

                if (index == -1)
                {
                    _logger.LogWarning("Replacement part with OE {OE} not found in part {Id}", replacementOe, partId);
                    return null;
                }

                // Update the replacement part
                part.ReplacementParts[index] = updatedReplacementPart;
                var result = await _jsonFileService.UpdateAsync(partId, part);

                _logger.LogInformation("Updated replacement part in: {Id}", partId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating replacement part in: {Id}", partId);
                throw;
            }
        }

        public async Task<Part?> RemoveReplacementPartAsync(string partId, string replacementOe)
        {
            try
            {
                var part = await _jsonFileService.GetByIdAsync(partId);
                if (part == null)
                {
                    _logger.LogWarning("Part not found: {Id}", partId);
                    return null;
                }

                var replacementPart = part.ReplacementParts.FirstOrDefault(rp => rp.ReplacementOe == replacementOe);
                if (replacementPart != null)
                {
                    part.ReplacementParts.Remove(replacementPart);
                    var result = await _jsonFileService.UpdateAsync(partId, part);
                    _logger.LogInformation("Removed replacement part from: {Id}", partId);
                    return result;
                }

                return part;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing replacement part from: {Id}", partId);
                throw;
            }
        }

        public async Task<Part?> UpdateAdaptableModelAsync(string partId, string modelCode, AdaptableModel updatedAdaptableModel)
        {
            try
            {
                var part = await _jsonFileService.GetByIdAsync(partId);
                if (part == null)
                {
                    _logger.LogWarning("Part not found: {Id}", partId);
                    return null;
                }

                var index = part.AdaptableModels.FindIndex(am => 
                    am.ModelCode.Equals(modelCode, StringComparison.OrdinalIgnoreCase));

                if (index == -1)
                {
                    _logger.LogWarning("Adaptable model with code {Code} not found in part {Id}", modelCode, partId);
                    return null;
                }

                // Update the adaptable model
                part.AdaptableModels[index] = updatedAdaptableModel;
                var result = await _jsonFileService.UpdateAsync(partId, part);

                _logger.LogInformation("Updated adaptable model in: {Id}", partId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating adaptable model in: {Id}", partId);
                throw;
            }
        }

        public async Task<Part?> RemoveAdaptableModelAsync(string partId, string modelCode)
        {
            try
            {
                var part = await _jsonFileService.GetByIdAsync(partId);
                if (part == null)
                {
                    _logger.LogWarning("Part not found: {Id}", partId);
                    return null;
                }

                var adaptableModel = part.AdaptableModels.FirstOrDefault(am => am.ModelCode == modelCode);
                if (adaptableModel != null)
                {
                    part.AdaptableModels.Remove(adaptableModel);
                    var result = await _jsonFileService.UpdateAsync(partId, part);
                    _logger.LogInformation("Removed adaptable model from: {Id}", partId);
                    return result;
                }

                return part;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing adaptable model from: {Id}", partId);
                throw;
            }
        }

        public async Task<List<Part>> BatchUpdateAsync(List<Part> parts)
        {
            try
            {
                if (parts == null || parts.Count == 0)
                {
                    _logger.LogWarning("Batch update called with empty or null parts list");
                    return new List<Part>();
                }

                _logger.LogInformation("Starting batch update for {Count} parts", parts.Count);

                var updatedParts = new List<Part>();
                var allParts = await _jsonFileService.GetAllAsync();

                foreach (var updatedPart in parts)
                {
                    try
                    {
                        var existingIndex = allParts.FindIndex(p => 
                            p.Id.Equals(updatedPart.Id, StringComparison.OrdinalIgnoreCase));

                        if (existingIndex >= 0)
                        {
                            // Update existing part
                            updatedPart.LastUpdated = DateTime.UtcNow;
                            allParts[existingIndex] = updatedPart;
                            updatedParts.Add(updatedPart);
                            _logger.LogInformation("Updated part: {Id}", updatedPart.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Part not found for update: {Id}", updatedPart.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating individual part: {Id}", updatedPart.Id);
                        // Continue with next part instead of failing entire batch
                    }
                }

                // Save all parts back to file
                await _jsonFileService.ReplaceAllAsync(allParts);

                _logger.LogInformation("Batch update completed. Updated {Count} parts", updatedParts.Count);
                return updatedParts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch update");
                throw;
            }
        }
    }
}
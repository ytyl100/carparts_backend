using CarPartsInventory.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarPartsInventory.API.Services
{
    public interface IPartService
    {
        Task<List<Part>> GetAllAsync();
        Task<Part?> GetByIdAsync(string id);
        Task<List<Part>> GetBySubCategoryIdAsync(string subCategoryId);
        Task<List<Part>> SearchAsync(PartSearchRequest request);
        Task<Part> CreateAsync(CreatePartRequest request);
        Task<Part?> UpdateAsync(string id, UpdatePartRequest request);
        Task<bool> DeleteAsync(string id);
        Task<List<Part>> GetByOeNumberAsync(string oeNumber);
        Task<List<Part>> GetByPositionAsync(string position);
        
        // 新增方法
        Task<List<Part>> GetByReplacementOeAsync(string oeNumber);
        Task<List<Part>> GetByModelCodeAsync(string modelCode);
        Task<List<Part>> GetByAdaptableBrandAsync(string brand);
        Task<Part?> AddReplacementPartAsync(string partId, ReplacementPart replacementPart);
        Task<Part?> AddAdaptableModelAsync(string partId, AdaptableModel adaptableModel);
        Task<Part?> UpdateReplacementPartAsync(string partId, string replacementOe, ReplacementPart updatedReplacementPart);
        Task<Part?> UpdateAdaptableModelAsync(string partId, string modelCode, AdaptableModel updatedAdaptableModel);
        Task<Part?> RemoveReplacementPartAsync(string partId, string replacementOe);
        Task<Part?> RemoveAdaptableModelAsync(string partId, string modelCode);

        // Batch operations
        Task<List<Part>> BatchUpdateAsync(List<Part> parts);
    }
}
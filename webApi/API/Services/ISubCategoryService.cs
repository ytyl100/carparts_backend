using CarPartsInventory.API.Models;
using CarPartsInventory.API.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarPartsInventory.API.Services
{
    public interface ISubCategoryService
    {
        Task<List<SubCategory>> GetAllAsync();
        Task<SubCategory?> GetByIdAsync(string id);
        Task<List<SubCategory>> GetByParentIdAsync(string parentId);
        Task<SubCategory> CreateAsync(CreateSubCategoryRequest request);
        Task<SubCategory?> UpdateAsync(string id, UpdateSubCategoryRequest request);
        Task<bool> DeleteAsync(string id);
        Task<List<SubCategory>> GetDefaultSubCategoriesAsync();
        Task<SubCategory?> UpdatePartialAsync(string id, SubCategoryUpdateDto dto);
    }
}
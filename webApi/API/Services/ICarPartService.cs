using System.Collections.Generic;
using System.Threading.Tasks;
using CarPartsInventory.API.Models;

namespace CarPartsInventory.API.Services
{
    public interface ICarPartService
    {
        Task<IEnumerable<Part>> GetAllPartsAsync();
        Task<Part?> GetPartByIdAsync(string id);
        Task<Part?> GetPartByPartNumberAsync(string oeNumber);
        Task<Part> CreatePartAsync(Part part);
        Task<Part?> UpdatePartAsync(string id, Part part);
        Task<bool> DeletePartAsync(string id);
        Task<IEnumerable<Part>> SearchPartsAsync(string searchTerm);
        Task<IEnumerable<Part>> GetPartsByCategoryAsync(string subCategoryId);
        Task<IEnumerable<Part>> GetLowStockPartsAsync(int threshold);
        Task<bool> UpdateStockAsync(string id, int quantityChange);
        Task<List<Part>> BatchUpdatePartsAsync(List<Part> parts);
    }
}
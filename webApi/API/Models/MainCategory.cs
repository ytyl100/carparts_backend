using System;
using System.ComponentModel.DataAnnotations;

namespace CarPartsInventory.API.Models
{
    public class MainCategory
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string Icon { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string VehicleCode { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class MainCategoryWrapper
    {
        public List<MainCategory> MainCategory { get; set; } = new();
    }

    // DTO for creation
    public class CreateMainCategoryRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        [Required]
        public string VehicleCode { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;
    }

    // DTO for update
    public class UpdateMainCategoryRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;
    }
}
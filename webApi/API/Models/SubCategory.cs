using System;
using System.ComponentModel.DataAnnotations;

namespace CarPartsInventory.API.Models
{
    public class SubCategory
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string ParentId { get; set; } = string.Empty; // MainCategory ID or VehicleCode

        [StringLength(500)]
        public string Image { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class SubCategoryWrapper
    {
        public List<SubCategory> SubCategory { get; set; } = new();
    }

    // DTO for creation
    public class CreateSubCategoryRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        [Required]
        public string ParentId { get; set; } = string.Empty;

        public string Image { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;
    }

    // DTO for update
    public class UpdateSubCategoryRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        [Required]
        public string ParentId { get; set; } = string.Empty;

        public string Image { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;
    }
}
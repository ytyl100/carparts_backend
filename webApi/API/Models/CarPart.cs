using System;
using System.ComponentModel.DataAnnotations;

namespace CarPartsInventory.API.Models
{
    // 为兼容旧引用，CarPart 继承标准 Part 模型
    public class CarPart : Part
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public string PartNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public string Manufacturer { get; set; }

        public string CarModel { get; set; }

        public int Year { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
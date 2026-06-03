using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarPartsInventory.API.Models
{
    public class Part
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string SubCategoryId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Position { get; set; } = string.Empty;

        // 🆕 新增字段：零部件编码（产品编码）
        [StringLength(100)]
        public string PartsNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string OeNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string StandardName { get; set; } = string.Empty;

        [StringLength(300)]
        public string OriginalName { get; set; } = string.Empty;

        // 🆕 新增字段：产地
        [StringLength(100)]
        public string Origin { get; set; } = string.Empty;

        [StringLength(20)]
        public string Quantity { get; set; } = string.Empty;

        [StringLength(200)]
        public string Note { get; set; } = string.Empty;

        [StringLength(50)]
        public string Date { get; set; } = string.Empty;

        public double X { get; set; }

        public double Y { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        // 🆕 新增字段：品牌（如 BYD、长安等）
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        // 🆕 新增字段：型号（如 海豚、汉等）
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        // 🆕 新增字段：车型（如 A05 2025）
        [StringLength(100)]
        public string CarModel { get; set; } = string.Empty;

        public List<PriceRecord> PriceRecords { get; set; } = new();

        public List<ReplacementPart> ReplacementParts { get; set; } = new();

        public List<AdaptableModel> AdaptableModels { get; set; } = new();

        public DateTime LastUpdated { get; internal set; }
    }

    public class PriceRecord
    {
        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Manufacturer { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal CostExclTax { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CostInclTax { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SaleExclTax { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SaleInclTax { get; set; }

        // 🆕 新增字段：货币单位（修改名字从 Currency 改为 Unit）
        [StringLength(10)]
        public string Unit { get; set; } = "RMB";
    }

    /// <summary>
    /// 替换配件信息
    /// </summary>
    public class ReplacementPart
    {
        /// <summary>
        /// 品牌
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        /// <summary>
        /// 原始OE号
        /// </summary>
        [Required]
        [StringLength(100)]
        public string OriginalOe { get; set; } = string.Empty;

        /// <summary>
        /// 替换OE号
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ReplacementOe { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(200)]
        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// 不含税成本价
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CostExclTax { get; set; } = 0;

        /// <summary>
        /// 含税成本价
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CostInclTax { get; set; } = 0;

        /// <summary>
        /// 不含税售价
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal SaleExclTax { get; set; } = 0;

        /// <summary>
        /// 含税售价
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal SaleInclTax { get; set; } = 0;

        // 🆕 新增字段：货币单位
        [StringLength(10)]
        public string Unit { get; set; } = "RMB";
    }

    /// <summary>
    /// 适配车型信息
    /// </summary>
    public class AdaptableModel
    {
        /// <summary>
        /// 品牌
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        /// <summary>
        /// 地区
        /// </summary>
        [StringLength(100)]
        public string Region { get; set; } = string.Empty;

        /// <summary>
        /// 车型名称
        /// </summary>
        [Required]
        [StringLength(200)]
        public string ModelName { get; set; } = string.Empty;

        /// <summary>
        /// 生产日期
        /// </summary>
        [StringLength(100)]
        public string ProductionDate { get; set; } = string.Empty;

        /// <summary>
        /// 车型代码
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ModelCode { get; set; } = string.Empty;

        /// <summary>
        /// 不含税成本价
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CostExclTax { get; set; } = 0;

        /// <summary>
        /// 含税成本价
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CostInclTax { get; set; } = 0;

        /// <summary>
        /// 不含税售价
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal SaleExclTax { get; set; } = 0;

        /// <summary>
        /// 含税售价
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal SaleInclTax { get; set; } = 0;

        // 🆕 新增字段：货币单位
        [StringLength(10)]
        public string Unit { get; set; } = "RMB";
    }

    public class PartWrapper
    {
        public List<Part> Part { get; set; } = new();
    }

    // DTO for creation
    public class CreatePartRequest
    {
        [Required]
        public string SubCategoryId { get; set; } = string.Empty;

        [Required]
        public string Position { get; set; } = string.Empty;

        // 🆕 新增
        public string PartsNumber { get; set; } = string.Empty;

        [Required]
        public string OeNumber { get; set; } = string.Empty;

        [Required]
        public string StandardName { get; set; } = string.Empty;

        public string OriginalName { get; set; } = string.Empty;

        // 🆕 新增
        public string Origin { get; set; } = string.Empty;

        public string Quantity { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;

        public string Date { get; set; } = string.Empty;

        public int X { get; set; }

        public int Y { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        // 🆕 新增
        public string Brand { get; set; } = string.Empty;

        // 🆕 新增
        public string Model { get; set; } = string.Empty;

        // 🆕 新增
        public string CarModel { get; set; } = string.Empty;

        public List<PriceRecord> PriceRecords { get; set; } = new();

        public List<ReplacementPart> ReplacementParts { get; set; } = new();

        public List<AdaptableModel> AdaptableModels { get; set; } = new();
    }

    // DTO for update
    public class UpdatePartRequest
    {
        [Required]
        public string SubCategoryId { get; set; } = string.Empty;

        [Required]
        public string Position { get; set; } = string.Empty;

        // 🆕 新增
        public string PartsNumber { get; set; } = string.Empty;

        [Required]
        public string OeNumber { get; set; } = string.Empty;

        [Required]
        public string StandardName { get; set; } = string.Empty;

        public string OriginalName { get; set; } = string.Empty;

        // 🆕 新增
        public string Origin { get; set; } = string.Empty;

        public string Quantity { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;

        public string Date { get; set; } = string.Empty;

        public int X { get; set; }

        public int Y { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        // 🆕 新增
        public string Brand { get; set; } = string.Empty;

        // 🆕 新增
        public string Model { get; set; } = string.Empty;

        // 🆕 新增
        public string CarModel { get; set; } = string.Empty;

        public List<PriceRecord> PriceRecords { get; set; } = new();

        public List<ReplacementPart> ReplacementParts { get; set; } = new();

        public List<AdaptableModel> AdaptableModels { get; set; } = new();
    }

    // DTO for search
    public class PartSearchRequest
    {
        public string? SubCategoryId { get; set; }

        // 🆕 新增：按零部件编码搜索
        public string? PartsNumber { get; set; }

        public string? OeNumber { get; set; }
        public string? StandardName { get; set; }
        public string? Position { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Brand { get; set; }

        // 🆕 新增：按型号搜索
        public string? Model { get; set; }

        // 🆕 新增：按车型搜索
        public string? CarModel { get; set; }

        public string? ModelCode { get; set; }
        public string? ReplacementOe { get; set; }
    }

    // DTO for querying replacement parts
    public class ReplacementPartQueryRequest
    {
        public string? Brand { get; set; }
        public string? OriginalOe { get; set; }
        public string? ReplacementOe { get; set; }
    }

    // DTO for querying adaptable models
    public class AdaptableModelQueryRequest
    {
        public string? Brand { get; set; }
        public string? Region { get; set; }
        public string? ModelName { get; set; }
        public string? ModelCode { get; set; }
    }
}
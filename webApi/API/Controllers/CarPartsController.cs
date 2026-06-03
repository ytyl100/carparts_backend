using CarPartsInventory.API.Models;
using CarPartsInventory.API.Models.DTOs;
using CarPartsInventory.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarPartsInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CarPartsController : ControllerBase
    {
        private readonly ICarPartService _carPartService;
        private readonly ISubCategoryService _subCategoryService;

        public CarPartsController(
            ICarPartService carPartService,
            ISubCategoryService subCategoryService)
        {
            _carPartService = carPartService;
            _subCategoryService = subCategoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Part>>> GetCarParts()
        {
            var parts = await _carPartService.GetAllPartsAsync();
            return Ok(parts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Part>> GetCarPart(string id)
        {
            var part = await _carPartService.GetPartByIdAsync(id);
            if (part == null) return NotFound();
            return Ok(part);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Part>>> SearchCarParts([FromQuery] string term)
        {
            var parts = await _carPartService.SearchPartsAsync(term);
            return Ok(parts);
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<Part>>> GetCarPartsByCategory(string category)
        {
            var parts = await _carPartService.GetPartsByCategoryAsync(category);
            return Ok(parts);
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<Part>>> GetLowStockParts([FromQuery] int threshold = 10)
        {
            var parts = await _carPartService.GetLowStockPartsAsync(threshold);
            return Ok(parts);
        }

        [HttpPost]
        public async Task<ActionResult<Part>> CreateCarPart(Part part)
        {
            try
            {
                var createdPart = await _carPartService.CreatePartAsync(part);
                return CreatedAtAction(nameof(GetCarPart), new { id = createdPart.Id }, createdPart);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCarPart(string id, Part part)
        {
            if (id != part.Id) return BadRequest();

            var updatedPart = await _carPartService.UpdatePartAsync(id, part);
            if (updatedPart == null) return NotFound();

            return NoContent();
        }

        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> UpdateStock(string id, [FromBody] StockUpdateRequest request)
        {
            var result = await _carPartService.UpdateStockAsync(id, request.QuantityChange);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCarPart(string id)
        {
            var result = await _carPartService.DeletePartAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPost("batch-update")]
        public async Task<ActionResult<object>> BatchUpdate(BatchUpdateRequest request)
        {
            try
            {
                // 验证请求
                if (request == null)
                {
                    return BadRequest(new 
                    { 
                        Success = false, 
                        Message = "Request body is required" 
                    });
                }

                var result = new
                {
                    UpdatedParts = new List<Part>(),
                    UpdatedSubCategory = (SubCategory?)null,
                    Success = true,
                    Message = "Batch update completed successfully"
                };

                // 处理零件批量更新
                if (request.UPDATED_PARTS != null && request.UPDATED_PARTS.Count > 0)
                {
                    var updatedParts = await _carPartService.BatchUpdatePartsAsync(request.UPDATED_PARTS);
                    result = result with { UpdatedParts = updatedParts };
                }

                // 处理子分类更新
                if (request.SUB_CATEGORIES_UPDATE != null && !string.IsNullOrEmpty(request.SUB_CATEGORIES_UPDATE.Id))
                {
                    var updatedSubCategory = await _subCategoryService.UpdatePartialAsync(
                        request.SUB_CATEGORIES_UPDATE.Id,
                        request.SUB_CATEGORIES_UPDATE);

                    if (updatedSubCategory == null)
                    {
                        return BadRequest(new 
                        { 
                            Success = false, 
                            Message = $"SubCategory with ID '{request.SUB_CATEGORIES_UPDATE.Id}' not found" 
                        });
                    }

                    result = result with { UpdatedSubCategory = updatedSubCategory };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new 
                { 
                    Success = false, 
                    Message = "Batch update failed",
                    Error = ex.Message 
                });
            }
        }

        /// <summary>
        /// 高级搜索：支持多条件组合搜索
        /// POST /api/carparts/search-advanced
        /// </summary>
        [HttpPost("search-advanced")]
        public async Task<ActionResult<IEnumerable<Part>>> SearchCarPartsAdvanced([FromBody] PartSearchRequest request)
        {
            var parts = await _carPartService.GetAllPartsAsync();
            
            var result = parts.AsEnumerable();

            // 按零件编码搜索
            if (!string.IsNullOrWhiteSpace(request.PartsNumber))
                result = result.Where(p => p.PartsNumber?.ToLowerInvariant()
                    .Contains(request.PartsNumber.ToLowerInvariant()) ?? false);

            // 按OE号搜索
            if (!string.IsNullOrWhiteSpace(request.OeNumber))
                result = result.Where(p => p.OeNumber.ToLowerInvariant()
                    .Contains(request.OeNumber.ToLowerInvariant()));

            // 按产品名称搜索
            if (!string.IsNullOrWhiteSpace(request.StandardName))
                result = result.Where(p => p.StandardName.ToLowerInvariant()
                    .Contains(request.StandardName.ToLowerInvariant()));

            // 按品牌搜索
            if (!string.IsNullOrWhiteSpace(request.Brand))
                result = result.Where(p => p.Brand?.ToLowerInvariant()
                    .Contains(request.Brand.ToLowerInvariant()) ?? false);

            // 按型号搜索
            if (!string.IsNullOrWhiteSpace(request.Model))
                result = result.Where(p => p.Model?.ToLowerInvariant()
                    .Contains(request.Model.ToLowerInvariant()) ?? false);

            // 按车型搜索
            if (!string.IsNullOrWhiteSpace(request.CarModel))
                result = result.Where(p => p.CarModel?.ToLowerInvariant()
                    .Contains(request.CarModel.ToLowerInvariant()) ?? false);

            // 按价格范围搜索
            if (request.MinPrice.HasValue)
                result = result.Where(p => p.PriceRecords.Any(pr => pr.SaleInclTax >= request.MinPrice.Value));

            if (request.MaxPrice.HasValue)
                result = result.Where(p => p.PriceRecords.Any(pr => pr.SaleInclTax <= request.MaxPrice.Value));

            return Ok(result.ToList());
        }
    }

    public class StockUpdateRequest
    {
        public int QuantityChange { get; set; }
    }
}
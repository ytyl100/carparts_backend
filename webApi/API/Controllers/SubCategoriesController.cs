using CarPartsInventory.API.Models;
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
    public class SubCategoriesController : ControllerBase
    {
        private readonly ISubCategoryService _subCategoryService;
        private readonly ILogger<SubCategoriesController> _logger;

        public SubCategoriesController(
            ISubCategoryService subCategoryService,
            ILogger<SubCategoriesController> logger)
        {
            _subCategoryService = subCategoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all sub categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<SubCategory>>> GetAll()
        {
            try
            {
                var categories = await _subCategoryService.GetAllAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all sub categories");
                return StatusCode(500, new { message = "An error occurred while getting sub categories." });
            }
        }

        /// <summary>
        /// Get sub category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SubCategory>> GetById(string id)
        {
            try
            {
                var category = await _subCategoryService.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = $"Sub category with ID {id} not found." });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sub category by ID");
                return StatusCode(500, new { message = "An error occurred while getting sub category." });
            }
        }

        /// <summary>
        /// Get sub categories by parent ID (MainCategory or VehicleCode)
        /// </summary>
        [HttpGet("parent/{parentId}")]
        public async Task<ActionResult<List<SubCategory>>> GetByParentId(string parentId)
        {
            try
            {
                var categories = await _subCategoryService.GetByParentIdAsync(parentId);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sub categories by parent ID");
                return StatusCode(500, new { message = "An error occurred while getting sub categories." });
            }
        }

        /// <summary>
        /// Get default sub categories
        /// </summary>
        [HttpGet("defaults")]
        public async Task<ActionResult<List<SubCategory>>> GetDefaults()
        {
            try
            {
                var categories = await _subCategoryService.GetDefaultSubCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default sub categories");
                return StatusCode(500, new { message = "An error occurred while getting default sub categories." });
            }
        }

        /// <summary>
        /// Create a new sub category
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<SubCategory>> Create([FromBody] CreateSubCategoryRequest request)
        {
            try
            {
                var category = await _subCategoryService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sub category");
                return StatusCode(500, new { message = "An error occurred while creating sub category." });
            }
        }

        /// <summary>
        /// Update a sub category
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<SubCategory>> Update(string id, [FromBody] UpdateSubCategoryRequest request)
        {
            try
            {
                var category = await _subCategoryService.UpdateAsync(id, request);
                if (category == null)
                {
                    return NotFound(new { message = $"Sub category with ID {id} not found." });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sub category");
                return StatusCode(500, new { message = "An error occurred while updating sub category." });
            }
        }

        /// <summary>
        /// Delete a sub category
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _subCategoryService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Sub category with ID {id} not found." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sub category");
                return StatusCode(500, new { message = "An error occurred while deleting sub category." });
            }
        }
    }
}
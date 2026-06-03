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
    public class MainCategoriesController : ControllerBase
    {
        private readonly IMainCategoryService _mainCategoryService;
        private readonly ILogger<MainCategoriesController> _logger;

        public MainCategoriesController(
            IMainCategoryService mainCategoryService,
            ILogger<MainCategoriesController> logger)
        {
            _mainCategoryService = mainCategoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all main categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<MainCategory>>> GetAll()
        {
            try
            {
                var categories = await _mainCategoryService.GetAllAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all main categories");
                return StatusCode(500, new { message = "An error occurred while getting main categories." });
            }
        }

        /// <summary>
        /// Get main category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MainCategory>> GetById(string id)
        {
            try
            {
                var category = await _mainCategoryService.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { message = $"Main category with ID {id} not found." });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting main category by ID");
                return StatusCode(500, new { message = "An error occurred while getting main category." });
            }
        }

        /// <summary>
        /// Get main categories by vehicle code
        /// </summary>
        [HttpGet("vehicle/{vehicleCode}")]
        public async Task<ActionResult<List<MainCategory>>> GetByVehicleCode(string vehicleCode)
        {
            try
            {
                var categories = await _mainCategoryService.GetByVehicleCodeAsync(vehicleCode);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting main categories by vehicle code");
                return StatusCode(500, new { message = "An error occurred while getting main categories." });
            }
        }

        /// <summary>
        /// Get default main categories
        /// </summary>
        [HttpGet("defaults")]
        public async Task<ActionResult<List<MainCategory>>> GetDefaults()
        {
            try
            {
                var categories = await _mainCategoryService.GetDefaultCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default main categories");
                return StatusCode(500, new { message = "An error occurred while getting default main categories." });
            }
        }

        /// <summary>
        /// Create a new main category
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<MainCategory>> Create([FromBody] CreateMainCategoryRequest request)
        {
            try
            {
                var category = await _mainCategoryService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating main category");
                return StatusCode(500, new { message = "An error occurred while creating main category." });
            }
        }

        /// <summary>
        /// Update a main category
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<MainCategory>> Update(string id, [FromBody] UpdateMainCategoryRequest request)
        {
            try
            {
                var category = await _mainCategoryService.UpdateAsync(id, request);
                if (category == null)
                {
                    return NotFound(new { message = $"Main category with ID {id} not found." });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating main category");
                return StatusCode(500, new { message = "An error occurred while updating main category." });
            }
        }

        /// <summary>
        /// Delete a main category
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _mainCategoryService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Main category with ID {id} not found." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting main category");
                return StatusCode(500, new { message = "An error occurred while deleting main category." });
            }
        }
    }
}
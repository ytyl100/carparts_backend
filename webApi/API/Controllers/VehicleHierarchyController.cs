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
    public class VehicleHierarchyController : ControllerBase
    {
        private readonly IVehicleHierarchyService _vehicleHierarchyService;
        private readonly ILogger<VehicleHierarchyController> _logger;

        public VehicleHierarchyController(
            IVehicleHierarchyService vehicleHierarchyService,
            ILogger<VehicleHierarchyController> logger)
        {
            _vehicleHierarchyService = vehicleHierarchyService;
            _logger = logger;
        }

        /// <summary>
        /// Get all vehicle hierarchy data
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<VehicleHierarchy>> GetAll()
        {
            try
            {
                var hierarchy = await _vehicleHierarchyService.GetAllAsync();
                return Ok(hierarchy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle hierarchy");
                return StatusCode(500, new { message = "An error occurred while getting vehicle hierarchy." });
            }
        }

        /// <summary>
        /// Get vehicle hierarchy by brand ID
        /// </summary>
        [HttpGet("brands/{brandId}")]
        public async Task<ActionResult<VehicleHierarchyDto>> GetByBrandId(string brandId)
        {
            try
            {
                var hierarchy = await _vehicleHierarchyService.GetByBrandIdAsync(brandId);
                if (hierarchy == null)
                {
                    return NotFound(new { message = $"Brand with ID {brandId} not found." });
                }
                return Ok(hierarchy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle hierarchy by brand ID");
                return StatusCode(500, new { message = "An error occurred while getting vehicle hierarchy." });
            }
        }

        /// <summary>
        /// Get all vehicle codes
        /// </summary>
        [HttpGet("codes")]
        public async Task<ActionResult<List<string>>> GetAllCodes()
        {
            try
            {
                var codes = await _vehicleHierarchyService.GetAllVehicleCodesAsync();
                return Ok(codes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all vehicle codes");
                return StatusCode(500, new { message = "An error occurred while getting vehicle codes." });
            }
        }

        /// <summary>
        /// Get vehicle information by code
        /// </summary>
        [HttpGet("codes/{code}")]
        public async Task<ActionResult<List<VehicleCodeQueryResult>>> GetByCode(string code)
        {
            try
            {
                var results = await _vehicleHierarchyService.GetVehiclesByCodeAsync(code);
                if (results.Count == 0)
                {
                    return NotFound(new { message = $"No vehicles found with code {code}." });
                }
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles by code");
                return StatusCode(500, new { message = "An error occurred while getting vehicles." });
            }
        }

        /// <summary>
        /// Get vehicle codes by brand
        /// </summary>
        [HttpGet("brands/{brandId}/codes")]
        public async Task<ActionResult<List<string>>> GetCodesByBrand(string brandId)
        {
            try
            {
                var codes = await _vehicleHierarchyService.GetVehicleCodesByBrandAsync(brandId);
                return Ok(codes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle codes by brand");
                return StatusCode(500, new { message = "An error occurred while getting vehicle codes." });
            }
        }

        /// <summary>
        /// Get vehicle codes by brand and region
        /// </summary>
        [HttpGet("brands/{brandId}/regions/{region}/codes")]
        public async Task<ActionResult<List<string>>> GetCodesByBrandAndRegion(string brandId, string region)
        {
            try
            {
                var codes = await _vehicleHierarchyService.GetVehicleCodesByBrandAndRegionAsync(brandId, region);
                return Ok(codes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle codes by brand and region");
                return StatusCode(500, new { message = "An error occurred while getting vehicle codes." });
            }
        }

        /// <summary>
        /// Get vehicle codes by brand and model
        /// </summary>
        [HttpGet("brands/{brandId}/models/{modelName}/codes")]
        public async Task<ActionResult<List<string>>> GetCodesByBrandAndModel(string brandId, string modelName)
        {
            try
            {
                var codes = await _vehicleHierarchyService.GetVehicleCodesByBrandAndModelAsync(brandId, modelName);
                return Ok(codes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle codes by brand and model");
                return StatusCode(500, new { message = "An error occurred while getting vehicle codes." });
            }
        }

        /// <summary>
        /// Get models by brand
        /// </summary>
        [HttpGet("brands/{brandId}/models")]
        public async Task<ActionResult<Dictionary<string, ModelData>>> GetModelsByBrand(string brandId)
        {
            try
            {
                var models = await _vehicleHierarchyService.GetModelsByBrandAsync(brandId);
                if (models == null)
                {
                    return NotFound(new { message = $"No models found for brand {brandId}." });
                }
                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting models by brand");
                return StatusCode(500, new { message = "An error occurred while getting models." });
            }
        }

        /// <summary>
        /// Get models by brand and region
        /// </summary>
        [HttpGet("brands/{brandId}/regions/{region}/models")]
        public async Task<ActionResult<Dictionary<string, ModelData>>> GetModelsByBrandAndRegion(
            string brandId, 
            string region)
        {
            try
            {
                var models = await _vehicleHierarchyService.GetModelsByBrandAndRegionAsync(brandId, region);
                if (models == null)
                {
                    return NotFound(new { message = $"No models found for brand {brandId} in region {region}." });
                }
                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting models by brand and region");
                return StatusCode(500, new { message = "An error occurred while getting models." });
            }
        }

        /// <summary>
        /// Create a new brand hierarchy
        /// </summary>
        [HttpPost("brands/{brandId}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<ActionResult<BrandHierarchy>> CreateBrand(
            string brandId, 
            [FromBody] BrandHierarchy brandData)
        {
            try
            {
                var brand = await _vehicleHierarchyService.CreateBrandAsync(brandId, brandData);
                return CreatedAtAction(
                    nameof(GetByBrandId), 
                    new { brandId = brandId }, 
                    brand);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand");
                return StatusCode(500, new { message = "An error occurred while creating brand." });
            }
        }

        /// <summary>
        /// Update a brand hierarchy
        /// </summary>
        [HttpPut("brands/{brandId}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<ActionResult<BrandHierarchy>> UpdateBrand(
            string brandId, 
            [FromBody] BrandHierarchy brandData)
        {
            try
            {
                var brand = await _vehicleHierarchyService.UpdateBrandAsync(brandId, brandData);
                if (brand == null)
                {
                    return NotFound(new { message = $"Brand with ID {brandId} not found." });
                }
                return Ok(brand);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand");
                return StatusCode(500, new { message = "An error occurred while updating brand." });
            }
        }

        /// <summary>
        /// Delete a brand hierarchy
        /// </summary>
        [HttpDelete("brands/{brandId}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> DeleteBrand(string brandId)
        {
            try
            {
                var result = await _vehicleHierarchyService.DeleteBrandAsync(brandId);
                if (!result)
                {
                    return NotFound(new { message = $"Brand with ID {brandId} not found." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand");
                return StatusCode(500, new { message = "An error occurred while deleting brand." });
            }
        }

        /// <summary>
        /// Add a region to a brand
        /// </summary>
        [HttpPost("brands/{brandId}/regions/{regionName}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<ActionResult<RegionData>> AddRegion(
            string brandId, 
            string regionName, 
            [FromBody] RegionData regionData)
        {
            try
            {
                var region = await _vehicleHierarchyService.AddRegionToBrandAsync(
                    brandId, 
                    regionName, 
                    regionData);
                return Ok(region);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding region");
                return StatusCode(500, new { message = "An error occurred while adding region." });
            }
        }

        /// <summary>
        /// Delete a region from a brand
        /// </summary>
        [HttpDelete("brands/{brandId}/regions/{regionName}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> DeleteRegion(string brandId, string regionName)
        {
            try
            {
                var result = await _vehicleHierarchyService.DeleteRegionFromBrandAsync(brandId, regionName);
                if (!result)
                {
                    return NotFound(new { message = $"Region {regionName} not found for brand {brandId}." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting region");
                return StatusCode(500, new { message = "An error occurred while deleting region." });
            }
        }

        /// <summary>
        /// Add a model to a brand's region
        /// </summary>
        [HttpPost("brands/{brandId}/regions/{regionName}/models/{modelName}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<ActionResult<ModelData>> AddModel(
            string brandId, 
            string regionName, 
            string modelName, 
            [FromBody] ModelData modelData)
        {
            try
            {
                var model = await _vehicleHierarchyService.AddModelAsync(
                    brandId, 
                    regionName, 
                    modelName, 
                    modelData);
                return Ok(model);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding model");
                return StatusCode(500, new { message = "An error occurred while adding model." });
            }
        }

        /// <summary>
        /// Add a model directly to a brand (without region)
        /// </summary>
        [HttpPost("brands/{brandId}/models/{modelName}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<ActionResult<ModelData>> AddModelDirectly(
            string brandId, 
            string modelName, 
            [FromBody] ModelData modelData)
        {
            try
            {
                var model = await _vehicleHierarchyService.AddModelDirectlyAsync(
                    brandId, 
                    modelName, 
                    modelData);
                return Ok(model);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding model");
                return StatusCode(500, new { message = "An error occurred while adding model." });
            }
        }

        /// <summary>
        /// Delete a model from a brand's region
        /// </summary>
        [HttpDelete("brands/{brandId}/regions/{regionName}/models/{modelName}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> DeleteModel(
            string brandId, 
            string regionName, 
            string modelName)
        {
            try
            {
                var result = await _vehicleHierarchyService.DeleteModelAsync(
                    brandId, 
                    regionName, 
                    modelName);
                if (!result)
                {
                    return NotFound(new { message = $"Model {modelName} not found." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting model");
                return StatusCode(500, new { message = "An error occurred while deleting model." });
            }
        }

        /// <summary>
        /// Delete a model directly from a brand (without region)
        /// </summary>
        [HttpDelete("brands/{brandId}/models/{modelName}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> DeleteModelDirectly(string brandId, string modelName)
        {
            try
            {
                var result = await _vehicleHierarchyService.DeleteModelDirectlyAsync(brandId, modelName);
                if (!result)
                {
                    return NotFound(new { message = $"Model {modelName} not found." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting model");
                return StatusCode(500, new { message = "An error occurred while deleting model." });
            }
        }
    }
}
using CarPartsInventory.API.Models;
using CarPartsInventory.API.Models.DTOs;
using CarPartsInventory.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarPartsInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _partService;
        private readonly ILogger<PartsController> _logger;

        public PartsController(
            IPartService partService,
            ILogger<PartsController> logger)
        {
            _partService = partService;
            _logger = logger;
        }

        /// <summary>
        /// Get all parts
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Part>>> GetAll()
        {
            try
            {
                var parts = await _partService.GetAllAsync();
                return Ok(parts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all parts");
                return StatusCode(500, new { message = "An error occurred while getting parts." });
            }
        }

        /// <summary>
        /// Get part by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Part>> GetById(string id)
        {
            try
            {
                var part = await _partService.GetByIdAsync(id);
                if (part == null)
                {
                    return NotFound(new { message = $"Part with ID {id} not found." });
                }
                return Ok(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part by ID");
                return StatusCode(500, new { message = "An error occurred while getting part." });
            }
        }

        /// <summary>
        /// Get parts by sub category ID
        /// </summary>
        [HttpGet("subcategory/{subCategoryId}")]
        public async Task<ActionResult<List<Part>>> GetBySubCategoryId(string subCategoryId)
        {
            try
            {
                var parts = await _partService.GetBySubCategoryIdAsync(subCategoryId);
                return Ok(parts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts by sub category ID");
                return StatusCode(500, new { message = "An error occurred while getting parts." });
            }
        }

        /// <summary>
        /// Get parts by OE number
        /// </summary>
        [HttpGet("oe/{oeNumber}")]
        public async Task<ActionResult<List<Part>>> GetByOeNumber(string oeNumber)
        {
            try
            {
                var parts = await _partService.GetByOeNumberAsync(oeNumber);
                return Ok(parts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts by OE number");
                return StatusCode(500, new { message = "An error occurred while getting parts." });
            }
        }

        /// <summary>
        /// Get parts by position
        /// </summary>
        [HttpGet("position/{position}")]
        public async Task<ActionResult<List<Part>>> GetByPosition(string position)
        {
            try
            {
                var parts = await _partService.GetByPositionAsync(position);
                return Ok(parts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts by position");
                return StatusCode(500, new { message = "An error occurred while getting parts." });
            }
        }

        /// <summary>
        /// Get parts by replacement OE number
        /// </summary>
        [HttpGet("replacement/{oeNumber}")]
        public async Task<ActionResult<List<Part>>> GetByReplacementOe(string oeNumber)
        {
            try
            {
                var parts = await _partService.GetByReplacementOeAsync(oeNumber);
                return Ok(parts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts by replacement OE");
                return StatusCode(500, new { message = "An error occurred while getting parts." });
            }
        }

        /// <summary>
        /// Get parts by model code
        /// </summary>
        [HttpGet("model/{modelCode}")]
        public async Task<ActionResult<List<Part>>> GetByModelCode(string modelCode)
        {
            try
            {
                var parts = await _partService.GetByModelCodeAsync(modelCode);
                return Ok(parts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts by model code");
                return StatusCode(500, new { message = "An error occurred while getting parts." });
            }
        }

        /// <summary>
        /// Get parts by adaptable brand
        /// </summary>
        [HttpGet("brand/{brand}")]
        public async Task<ActionResult<List<Part>>> GetByAdaptableBrand(string brand)
        {
            try
            {
                var parts = await _partService.GetByAdaptableBrandAsync(brand);
                return Ok(parts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts by brand");
                return StatusCode(500, new { message = "An error occurred while getting parts." });
            }
        }

        /// <summary>
        /// Search parts with filters
        /// </summary>
        [HttpPost("search")]
        public async Task<ActionResult<List<Part>>> Search([FromBody] PartSearchRequest request)
        {
            try
            {
                var parts = await _partService.SearchAsync(request);
                return Ok(parts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching parts");
                return StatusCode(500, new { message = "An error occurred while searching parts." });
            }
        }

        /// <summary>
        /// Create a new part
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<Part>> Create([FromBody] CreatePartRequest request)
        {
            try
            {
                var part = await _partService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = part.Id }, part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating part");
                return StatusCode(500, new { message = "An error occurred while creating part." });
            }
        }

        /// <summary>
        /// Update a part
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<Part>> Update(string id, [FromBody] UpdatePartRequest request)
        {
            try
            {
                var part = await _partService.UpdateAsync(id, request);
                if (part == null)
                {
                    return NotFound(new { message = $"Part with ID {id} not found." });
                }
                return Ok(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part");
                return StatusCode(500, new { message = "An error occurred while updating part." });
            }
        }

        /// <summary>
        /// Batch update multiple parts
        /// </summary>
        [HttpPut("batch")]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<List<Part>>> BatchUpdate([FromBody] BatchUpdateRequest request)
        {
            try
            {
                if (request.UPDATED_PARTS == null || request.UPDATED_PARTS.Count == 0)
                {
                    return BadRequest(new { message = "No parts provided for update." });
                }

                _logger.LogInformation("Batch update request received for {Count} parts", request.UPDATED_PARTS.Count);

                var updatedParts = await _partService.BatchUpdateAsync(request.UPDATED_PARTS);

                return Ok(new 
                { 
                    message = $"Successfully updated {updatedParts.Count} parts.",
                    updatedCount = updatedParts.Count,
                    parts = updatedParts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch update");
                return StatusCode(500, new { message = "An error occurred during batch update.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a part
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _partService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Part with ID {id} not found." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting part");
                return StatusCode(500, new { message = "An error occurred while deleting part." });
            }
        }

        /// <summary>
        /// Add replacement part info to a part
        /// </summary>
        [HttpPost("{id}/replacements")]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<Part>> AddReplacementPart(
            string id, 
            [FromBody] ReplacementPart replacementPart)
        {
            try
            {
                var part = await _partService.AddReplacementPartAsync(id, replacementPart);
                if (part == null)
                {
                    return NotFound(new { message = $"Part with ID {id} not found." });
                }
                return Ok(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding replacement part");
                return StatusCode(500, new { message = "An error occurred while adding replacement part." });
            }
        }

        /// <summary>
        /// Get all replacement parts for a part
        /// </summary>
        [HttpGet("{id}/replacements")]
        public async Task<ActionResult<List<ReplacementPart>>> GetReplacementParts(string id)
        {
            try
            {
                var part = await _partService.GetByIdAsync(id);
                if (part == null)
                {
                    return NotFound(new { message = $"Part with ID {id} not found." });
                }
                return Ok(part.ReplacementParts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting replacement parts");
                return StatusCode(500, new { message = "An error occurred while getting replacement parts." });
            }
        }

        /// <summary>
        /// Get a specific replacement part
        /// </summary>
        [HttpGet("{id}/replacements/{replacementOe}")]
        public async Task<ActionResult<ReplacementPart>> GetReplacementPart(string id, string replacementOe)
        {
            try
            {
                var part = await _partService.GetByIdAsync(id);
                if (part == null)
                {
                    return NotFound(new { message = $"Part with ID {id} not found." });
                }

                var replacementPart = part.ReplacementParts.FirstOrDefault(r => 
                    r.ReplacementOe.Equals(replacementOe, StringComparison.OrdinalIgnoreCase));

                if (replacementPart == null)
                {
                    return NotFound(new { message = $"Replacement part with OE {replacementOe} not found." });
                }

                return Ok(replacementPart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting replacement part");
                return StatusCode(500, new { message = "An error occurred while getting replacement part." });
            }
        }

        /// <summary>
        /// Add adaptable model info to a part
        /// </summary>
        [HttpPost("{id}/models")]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<Part>> AddAdaptableModel(
            string id, 
            [FromBody] AdaptableModel adaptableModel)
        {
            try
            {
                var part = await _partService.AddAdaptableModelAsync(id, adaptableModel);
                if (part == null)
                {
                    return NotFound(new { message = $"Part with ID {id} not found." });
                }
                return Ok(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding adaptable model");
                return StatusCode(500, new { message = "An error occurred while adding adaptable model." });
            }
        }

        /// <summary>
        /// Get all adaptable models for a part
        /// </summary>
        [HttpGet("{id}/models")]
        public async Task<ActionResult<List<AdaptableModel>>> GetAdaptableModels(string id)
        {
            try
            {
                var part = await _partService.GetByIdAsync(id);
                if (part == null)
                {
                    return NotFound(new { message = $"Part with ID {id} not found." });
                }
                return Ok(part.AdaptableModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting adaptable models");
                return StatusCode(500, new { message = "An error occurred while getting adaptable models." });
            }
        }

        /// <summary>
        /// Get a specific adaptable model
        /// </summary>
        [HttpGet("{id}/models/{modelCode}")]
        public async Task<ActionResult<AdaptableModel>> GetAdaptableModel(string id, string modelCode)
        {
            try
            {
                var part = await _partService.GetByIdAsync(id);
                if (part == null)
                {
                    return NotFound(new { message = $"Part with ID {id} not found." });
                }

                var adaptableModel = part.AdaptableModels.FirstOrDefault(m => 
                    m.ModelCode.Equals(modelCode, StringComparison.OrdinalIgnoreCase));

                if (adaptableModel == null)
                {
                    return NotFound(new { message = $"Adaptable model with code {modelCode} not found." });
                }

                return Ok(adaptableModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting adaptable model");
                return StatusCode(500, new { message = "An error occurred while getting adaptable model." });
            }
        }

        /// <summary>
        /// Update replacement part info for a part
        /// </summary>
        [HttpPut("{id}/replacements/{replacementOe}")]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<Part>> UpdateReplacementPart(
            string id, 
            string replacementOe,
            [FromBody] ReplacementPart replacementPart)
        {
            try
            {
                var part = await _partService.UpdateReplacementPartAsync(id, replacementOe, replacementPart);
                if (part == null)
                {
                    return NotFound(new { message = $"Part with ID {id} or replacement part with OE {replacementOe} not found." });
                }
                return Ok(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating replacement part");
                return StatusCode(500, new { message = "An error occurred while updating replacement part." });
            }
        }

        /// <summary>
        /// Remove replacement part info from a part
        /// </summary>
        [HttpDelete("{id}/replacements/{replacementOe}")]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<Part>> RemoveReplacementPart(string id, string replacementOe)
        {
            try
            {
                var part = await _partService.RemoveReplacementPartAsync(id, replacementOe);
                if (part == null)
                {
                    return NotFound(new { message = $"Part with ID {id} not found." });
                }
                return Ok(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing replacement part");
                return StatusCode(500, new { message = "An error occurred while removing replacement part." });
            }
        }

        /// <summary>
        /// Update adaptable model info for a part
        /// </summary>
        [HttpPut("{id}/models/{modelCode}")]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<Part>> UpdateAdaptableModel(
            string id, 
            string modelCode,
            [FromBody] AdaptableModel adaptableModel)
        {
            try
            {
                var part = await _partService.UpdateAdaptableModelAsync(id, modelCode, adaptableModel);
                if (part == null)
                {
                    return NotFound(new { message = $"Part with ID {id} or adaptable model with code {modelCode} not found." });
                }
                return Ok(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating adaptable model");
                return StatusCode(500, new { message = "An error occurred while updating adaptable model." });
            }
        }

        /// <summary>
        /// Remove adaptable model info from a part
        /// </summary>
        [HttpDelete("{id}/models/{modelCode}")]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<Part>> RemoveAdaptableModel(string id, string modelCode)
        {
            try
            {
                var part = await _partService.RemoveAdaptableModelAsync(id, modelCode);
                if (part == null)
                {
                    return NotFound(new { message = $"Part with ID {id} not found." });
                }
                return Ok(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing adaptable model");
                return StatusCode(500, new { message = "An error occurred while removing adaptable model." });
            }
        }
    }
}
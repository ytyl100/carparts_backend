using CarPartsInventory.API.Models;
using CarPartsInventory.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarPartsInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;
        private readonly ILogger<BrandsController> _logger;

        public BrandsController(IBrandService brandService, ILogger<BrandsController> logger)
        {
            _brandService = brandService;
            _logger = logger;
        }

        // GET: api/brands
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrands()
        {
            try
            {
                var userName = User.Identity?.Name;
                _logger.LogInformation("User {UserName} is getting brands", userName);
                
                var brands = await _brandService.GetAllBrandsAsync();
                return Ok(brands);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brands");
                return StatusCode(500, new { message = "An error occurred while getting brands." });
            }
        }

        // GET: api/brands/hot
        [HttpGet("hot")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Brand>>> GetHotBrands()
        {
            try
            {
                var hotBrands = await _brandService.GetHotBrandsAsync();
                return Ok(hotBrands);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hot brands");
                return StatusCode(500, new { message = "An error occurred while getting hot brands." });
            }
        }

        // GET: api/brands/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Brand>> GetBrand(string id)
        {
            try
            {
                var brand = await _brandService.GetBrandByIdAsync(id);
                if (brand == null)
                {
                    return NotFound(new { message = $"Brand with ID {id} not found." });
                }
                return Ok(brand);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand by ID");
                return StatusCode(500, new { message = "An error occurred while getting brand." });
            }
        }

        // GET: api/brands/name/{name}
        [HttpGet("name/{name}")]
        public async Task<ActionResult<Brand>> GetBrandByName(string name)
        {
            try
            {
                var brand = await _brandService.GetBrandByNameAsync(name);
                if (brand == null)
                {
                    return NotFound(new { message = $"Brand with name {name} not found." });
                }
                return Ok(brand);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand by name");
                return StatusCode(500, new { message = "An error occurred while getting brand." });
            }
        }

        // PUT: api/brands/batch — 批量替换所有品牌数据
        [HttpPut("batch")]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<BrandBatchResult>> ReplaceAllBrands([FromBody] List<Brand> brands)
        {
            try
            {
                if (brands == null || brands.Count == 0)
                {
                    return BadRequest(new { message = "品牌数组不能为空。" });
                }

                // 检查 id 重复
                var duplicateIds = brands
                    .Where(b => !string.IsNullOrEmpty(b.Id))
                    .GroupBy(b => b.Id)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateIds.Count > 0)
                {
                    return BadRequest(new { message = $"存在重复 ID：{string.Join(", ", duplicateIds)}" });
                }

                var userName = User.Identity?.Name;
                _logger.LogInformation("User {UserName} is replacing all brands with {Count} items", userName, brands.Count);

                var result = await _brandService.ReplaceAllBrandsAsync(brands);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing all brands");
                return StatusCode(500, new { message = "批量更新品牌时发生错误。" });
            }
        }

        // POST: api/brands
        [HttpPost]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<ActionResult<Brand>> CreateBrand(Brand brand)
        {
            try
            {
                var createdBrand = await _brandService.CreateBrandAsync(brand);
                return CreatedAtAction(nameof(GetBrand), new { id = createdBrand.Id }, createdBrand);
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

        // PUT: api/brands/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,Admin,manager,Manager")]
        public async Task<IActionResult> UpdateBrand(string id, Brand brand)
        {
            try
            {
                if (id != brand.Id)
                {
                    return BadRequest(new { message = "Brand ID mismatch." });
                }

                var updatedBrand = await _brandService.UpdateBrandAsync(id, brand);
                if (updatedBrand == null)
                {
                    return NotFound(new { message = $"Brand with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand");
                return StatusCode(500, new { message = "An error occurred while updating brand." });
            }
        }

        // DELETE: api/brands/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> DeleteBrand(string id)
        {
            try
            {
                var result = await _brandService.DeleteBrandAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Brand with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand");
                return StatusCode(500, new { message = "An error occurred while deleting brand." });
            }
        }

        // 🔍 调试端点 - 查看当前用户的 Claims
        [HttpGet("_debug/claims")]
        [AllowAnonymous]
        public IActionResult GetClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            var roles = User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value);

            return Ok(new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                UserName = User.Identity?.Name,
                Claims = claims,
                Roles = roles
            });
        }
    }
}
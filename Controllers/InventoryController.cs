using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WilliamMetalAPI.DTOs;
using WilliamMetalAPI.Services;

namespace WilliamMetalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] removed for testing — restore before production
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _inventoryService.GetInventoryStatsAsync();
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving inventory stats" });
            }
        }

        [HttpGet("movements")]
        public async Task<IActionResult> GetMovements([FromQuery] string? productId, [FromQuery] string? variantId)
        {
            try
            {
                var movements = await _inventoryService.GetInventoryMovementsAsync(productId, variantId);
                return Ok(new { success = true, data = movements });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving inventory movements" });
            }
        }

        [HttpGet("alerts")]
        public async Task<IActionResult> GetStockAlerts()
        {
            try
            {
                var alerts = await _inventoryService.GetStockAlertsAsync();
                return Ok(new { success = true, data = alerts });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving stock alerts" });
            }
        }

        [HttpPost("update-stock")]
        public async Task<IActionResult> UpdateStock([FromBody] StockMovementDto movement)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                var result = await _inventoryService.UpdateStockAsync(movement, userId);

                if (!result)
                {
                    return BadRequest(new { success = false, message = "Failed to update stock" });
                }

                return Ok(new { success = true, message = "Stock updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating stock" });
            }
        }

        [HttpPost("adjust-stock")]
        public async Task<IActionResult> AdjustStock([FromBody] StockAdjustmentDto adjustment)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                var result = await _inventoryService.AdjustStockAsync(adjustment, userId);

                if (!result)
                {
                    return BadRequest(new { success = false, message = "Failed to adjust stock" });
                }

                return Ok(new { success = true, message = "Stock adjusted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error adjusting stock" });
            }
        }

        [HttpGet("movements/{id}")]
        public async Task<IActionResult> GetMovement(string id)
        {
            try
            {
                var movement = await _inventoryService.GetMovementByIdAsync(id);
                if (movement == null)
                {
                    return NotFound(new { success = false, message = "Movement not found" });
                }

                return Ok(new { success = true, data = movement });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving movement" });
            }
        }
    }
}
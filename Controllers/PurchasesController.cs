using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WilliamMetalAPI.DTOs;
using WilliamMetalAPI.Services;

namespace WilliamMetalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchasesController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchases([FromQuery] PurchaseFilterDto filter)
        {
            try
            {
                var purchases = await _purchaseService.GetPurchasesAsync(filter);
                return Ok(new { success = true, data = purchases });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving purchases" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPurchase(string id)
        {
            try
            {
                var purchase = await _purchaseService.GetPurchaseByIdAsync(id);
                if (purchase == null)
                {
                    return NotFound(new { success = false, message = "Purchase not found" });
                }

                return Ok(new { success = true, data = purchase });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving purchase" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseDto createDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                var purchase = await _purchaseService.CreatePurchaseAsync(createDto, userId);

                return CreatedAtAction(nameof(GetPurchase), new { id = purchase.Id }, 
                    new { success = true, data = purchase, message = "Purchase created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error creating purchase" });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdatePurchaseStatus(string id, [FromBody] UpdatePurchaseStatusDto statusDto)
        {
            try
            {
                var result = await _purchaseService.UpdatePurchaseStatusAsync(id, statusDto);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Purchase not found" });
                }

                return Ok(new { success = true, message = "Purchase status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating purchase status" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchase(string id)
        {
            try
            {
                var result = await _purchaseService.DeletePurchaseAsync(id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Purchase not found" });
                }

                return Ok(new { success = true, message = "Purchase deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting purchase" });
            }
        }

        [HttpGet("suppliers")]
        public async Task<IActionResult> GetSuppliers()
        {
            try
            {
                var suppliers = await _purchaseService.GetSuppliersAsync();
                return Ok(new { success = true, data = suppliers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving suppliers" });
            }
        }

        [HttpPost("suppliers")]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto supplierDto)
        {
            try
            {
                var supplier = await _purchaseService.CreateSupplierAsync(supplierDto);
                return CreatedAtAction(nameof(GetSuppliers), new { id = supplier.Id }, 
                    new { success = true, data = supplier, message = "Supplier created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error creating supplier" });
            }
        }

        [HttpGet("purchase-number")]
        public async Task<IActionResult> GeneratePurchaseNumber()
        {
            try
            {
                var purchaseNumber = await _purchaseService.GeneratePurchaseNumberAsync();
                return Ok(new { success = true, data = purchaseNumber });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error generating purchase number" });
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WilliamMetalAPI.DTOs;
using WilliamMetalAPI.Services;

namespace WilliamMetalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSales([FromQuery] SaleFilterDto filter)
        {
            try
            {
                var sales = await _saleService.GetSalesAsync(filter);
                return Ok(new { success = true, data = sales });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving sales" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSale(string id)
        {
            try
            {
                var sale = await _saleService.GetSaleByIdAsync(id);
                if (sale == null)
                {
                    return NotFound(new { success = false, message = "Sale not found" });
                }

                return Ok(new { success = true, data = sale });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving sale" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto createDto)
        {
            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sale = await _saleService.CreateSaleAsync(createDto, userId);

                return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, 
                    new { success = true, data = sale, message = "Sale created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error creating sale" });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateSaleStatus(string id, [FromBody] UpdateStatusDto statusDto)
        {
            try
            {
                var result = await _saleService.UpdateSaleStatusAsync(id, statusDto.Status);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Sale not found" });
                }

                return Ok(new { success = true, message = "Sale status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating sale status" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSale(string id)
        {
            try
            {
                var result = await _saleService.DeleteSaleAsync(id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Sale not found" });
                }

                return Ok(new { success = true, message = "Sale deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting sale" });
            }
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var customers = await _saleService.GetCustomersAsync();
                return Ok(new { success = true, data = customers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving customers" });
            }
        }

        [HttpPost("customers")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto customerDto)
        {
            try
            {
                var customer = await _saleService.CreateCustomerAsync(customerDto);
                return CreatedAtAction(nameof(GetCustomers), new { id = customer.Id }, 
                    new { success = true, data = customer, message = "Customer created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error creating customer" });
            }
        }

        [HttpGet("invoice-number")]
        public async Task<IActionResult> GenerateInvoiceNumber()
        {
            try
            {
                var invoiceNumber = await _saleService.GenerateInvoiceNumberAsync();
                return Ok(new { success = true, data = invoiceNumber });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error generating invoice number" });
            }
        }

        public class UpdateStatusDto
        {
            public string Status { get; set; } = string.Empty;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using WilliamMetalAPI.Services;

namespace WilliamMetalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            try
            {
                var invoices = await _invoiceService.GetInvoicesAsync();
                return Ok(new { success = true, data = invoices });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving invoices" });
            }
        }

        [HttpGet("{saleId}")]
        public async Task<IActionResult> GetInvoice(string saleId)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceAsync(saleId);
                if (invoice == null)
                {
                    return NotFound(new { success = false, message = "Invoice not found" });
                }

                return Ok(new { success = true, data = invoice });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving invoice" });
            }
        }

        [HttpGet("{saleId}/pdf")]
        public async Task<IActionResult> GetInvoicePdf(string saleId)
        {
            try
            {
                var pdfResult = await _invoiceService.GetInvoicePdfAsync(saleId);
                if (pdfResult == null)
                {
                    return NotFound(new { success = false, message = "Invoice not found" });
                }

                return File(pdfResult.Value.Pdf, "application/pdf", $"invoice-{pdfResult.Value.InvoiceNumber}.pdf");
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error generating invoice PDF" });
            }
        }
    }
}

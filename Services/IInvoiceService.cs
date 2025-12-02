using WilliamMetalAPI.DTOs;

namespace WilliamMetalAPI.Services
{
    public interface IInvoiceService
    {
        Task<List<InvoiceDto>> GetInvoicesAsync();
        Task<InvoiceDto?> GetInvoiceAsync(string saleId);
        Task<(byte[] Pdf, string InvoiceNumber)?> GetInvoicePdfAsync(string saleId);
    }
}

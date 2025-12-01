using WilliamMetalAPI.DTOs;

namespace WilliamMetalAPI.Services
{
    public interface ISaleService
    {
        Task<List<SaleDto>> GetSalesAsync(SaleFilterDto filter);
        Task<SaleDto?> GetSaleByIdAsync(string id);
        Task<SaleDto> CreateSaleAsync(CreateSaleDto createDto, string? userId);
        Task<bool> UpdateSaleStatusAsync(string id, string status);
        Task<bool> DeleteSaleAsync(string id);
        Task<List<CustomerDto>> GetCustomersAsync();
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto customerDto);
        Task<string> GenerateInvoiceNumberAsync();
    }
}
using WilliamMetalAPI.DTOs;

namespace WilliamMetalAPI.Services
{
    public interface IPurchaseService
    {
        Task<List<PurchaseDto>> GetPurchasesAsync(PurchaseFilterDto filter);
        Task<PurchaseDto?> GetPurchaseByIdAsync(string id);
        Task<PurchaseDto> CreatePurchaseAsync(CreatePurchaseDto createDto, string? userId);
        Task<bool> UpdatePurchaseStatusAsync(string id, UpdatePurchaseStatusDto status);
        Task<bool> DeletePurchaseAsync(string id);
        Task<List<SupplierDto>> GetSuppliersAsync();
        Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto supplierDto);
        Task<string> GeneratePurchaseNumberAsync();
    }
}
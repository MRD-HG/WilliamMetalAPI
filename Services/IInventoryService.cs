using WilliamMetalAPI.DTOs;

namespace WilliamMetalAPI.Services
{
    public interface IInventoryService
    {
        Task<InventoryStatsDto> GetInventoryStatsAsync();
        Task<List<InventoryMovementDto>> GetInventoryMovementsAsync(string? productId = null, string? variantId = null);
        Task<List<StockAlertDto>> GetStockAlertsAsync();
        Task<bool> UpdateStockAsync(StockMovementDto movement, string? userId);
        Task<bool> AdjustStockAsync(StockAdjustmentDto adjustment, string? userId);
        Task<InventoryMovementDto?> GetMovementByIdAsync(string id);
    }
}
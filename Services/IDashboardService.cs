using WilliamMetalAPI.DTOs;

namespace WilliamMetalAPI.Services
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<DashboardDataDto> GetDashboardDataAsync();
        Task<List<SalesChartDataDto>> GetSalesChartDataAsync(int days = 30);
        Task<List<StockAlertDto>> GetStockAlertsAsync();
        Task<List<TopProductDto>> GetTopProductsAsync(int count = 10);
    }
}
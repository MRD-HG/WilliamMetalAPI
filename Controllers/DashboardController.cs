using Microsoft.AspNetCore.Mvc;
using WilliamMetalAPI.Services;

namespace WilliamMetalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] removed for testing — restore before production
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving dashboard stats" });
            }
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var data = await _dashboardService.GetDashboardDataAsync();
                return Ok(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving dashboard data" });
            }
        }

        [HttpGet("sales-chart")]
        public async Task<IActionResult> GetSalesChart([FromQuery] int days = 30)
        {
            try
            {
                var chartData = await _dashboardService.GetSalesChartDataAsync(days);
                return Ok(new { success = true, data = chartData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving sales chart data" });
            }
        }

        [HttpGet("stock-alerts")]
        public async Task<IActionResult> GetStockAlerts()
        {
            try
            {
                var alerts = await _dashboardService.GetStockAlertsAsync();
                return Ok(new { success = true, data = alerts });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving stock alerts" });
            }
        }

        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] int count = 10)
        {
            try
            {
                var topProducts = await _dashboardService.GetTopProductsAsync(count);
                return Ok(new { success = true, data = topProducts });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving top products" });
            }
        }
    }
}
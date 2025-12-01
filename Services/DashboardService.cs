using Microsoft.EntityFrameworkCore;
using WilliamMetalAPI.Data;
using WilliamMetalAPI.DTOs;
using WilliamMetalAPI.Models;

namespace WilliamMetalAPI.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly WilliamMetalContext _context;

        public DashboardService(WilliamMetalContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var todaySales = await _context.Sales
                .Where(s => s.CreatedAt.Date == today && s.Status == SaleStatus.COMPLETED)
                .SumAsync(s => s.Total);

            var todayPurchases = await _context.Purchases
                .Where(p => p.CreatedAt.Date == today)
                .SumAsync(p => p.Total);

            var allVariants = await _context.ProductVariants
                .Include(v => v.Product)
                .ToListAsync();

            var totalSales = await _context.Sales
                .Where(s => s.Status == SaleStatus.COMPLETED)
                .SumAsync(s => s.Total);

            var totalPurchases = await _context.Purchases
                .SumAsync(p => p.Total);

            var stats = new DashboardStatsDto
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalVariants = allVariants.Count,
                TotalStockValue = allVariants.Sum(v => v.Stock * v.Cost),
                TotalSales = totalSales,
                TotalPurchases = totalPurchases,
                StockAlerts = allVariants.Count(v => v.Stock <= v.MinStock),
                TodaySales = (int)todaySales,
                TodayPurchases = (int)todayPurchases
            };

            return stats;
        }

        public async Task<DashboardDataDto> GetDashboardDataAsync()
        {
            var stats = await GetDashboardStatsAsync();
            var salesChartData = await GetSalesChartDataAsync(30);
            var stockAlerts = await GetStockAlertsAsync();
            var topProducts = await GetTopProductsAsync(10);

            var recentActivities = new List<RecentActivityDto>();

            // Get recent sales
            var recentSales = await _context.Sales
                .Include(s => s.Customer)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToListAsync();

            foreach (var sale in recentSales)
            {
                recentActivities.Add(new RecentActivityDto
                {
                    Type = "SALE",
                    Reference = sale.InvoiceNumber,
                    Amount = sale.Total,
                    Date = sale.CreatedAt,
                    Status = sale.Status.ToString()
                });
            }

            // Get recent purchases
            var recentPurchases = await _context.Purchases
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            foreach (var purchase in recentPurchases)
            {
                recentActivities.Add(new RecentActivityDto
                {
                    Type = "PURCHASE",
                    Reference = purchase.PurchaseNumber,
                    Amount = purchase.Total,
                    Date = purchase.CreatedAt,
                    Status = purchase.PaymentStatus.ToString()
                });
            }

            // Get recent inventory movements
            var recentMovements = await _context.InventoryMovements
                .Include(m => m.Variant)
                .ThenInclude(v => v.Product)
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .ToListAsync();

            foreach (var movement in recentMovements)
            {
                recentActivities.Add(new RecentActivityDto
                {
                    Type = "INVENTORY",
                    Reference = $"{movement.Variant.Product.NameAr} - {movement.Variant.Specification}",
                    Amount = 0,
                    Date = movement.CreatedAt,
                    Status = movement.Type.ToString()
                });
            }

            recentActivities = recentActivities.OrderByDescending(a => a.Date).Take(10).ToList();

            return new DashboardDataDto
            {
                Stats = stats,
                SalesChartData = salesChartData,
                StockAlerts = stockAlerts,
                RecentActivities = recentActivities,
                TopProducts = topProducts
            };
        }

        public async Task<List<SalesChartDataDto>> GetSalesChartDataAsync(int days = 30)
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-days);

            // Materialize sales into memory first to avoid provider translation issues with Date properties.
            var salesList = await _context.Sales
                .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate && s.Status == SaleStatus.COMPLETED)
                .ToListAsync();

            var salesData = salesList
                .GroupBy(s => s.CreatedAt.Date)
                .Select(g => new SalesChartDataDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Amount = g.Sum(s => s.Total),
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Fill missing dates with zero values
            var chartData = new List<SalesChartDataDto>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dateStr = date.ToString("yyyy-MM-dd");
                var existingData = salesData.FirstOrDefault(d => d.Date == dateStr);

                chartData.Add(existingData ?? new SalesChartDataDto
                {
                    Date = dateStr,
                    Amount = 0,
                    Count = 0
                });
            }

            return chartData;
        }

        public async Task<List<StockAlertDto>> GetStockAlertsAsync()
        {
            var alerts = new List<StockAlertDto>();

            var variants = await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.Stock <= v.MinStock)
                .ToListAsync();

            foreach (var variant in variants)
            {
                alerts.Add(new StockAlertDto
                {
                    Product = variant.Product.NameAr,
                    Variant = variant.Specification,
                    CurrentStock = variant.Stock,
                    MinStock = variant.MinStock,
                    Type = variant.Stock == 0 ? "out_of_stock" : "low_stock"
                });
            }

            return alerts.OrderBy(a => a.Type).ThenBy(a => a.CurrentStock).ToList();
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(int count = 10)
        {
            var topProducts = await _context.SaleItems
                .Include(si => si.Variant)
                .ThenInclude(v => v.Product)
                .GroupBy(si => new { si.ProductId, si.VariantId })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.First().Variant.Product.NameAr,
                    VariantName = g.First().Variant.Specification,
                    TotalQuantity = g.Sum(si => si.Quantity),
                    TotalRevenue = g.Sum(si => si.TotalPrice)
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(count)
                .ToListAsync();

            return topProducts;
        }
    }
}
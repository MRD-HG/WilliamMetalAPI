namespace WilliamMetalAPI.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalProducts { get; set; }
        public int TotalVariants { get; set; }
        public decimal TotalStockValue { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalPurchases { get; set; }
        public int StockAlerts { get; set; }
        public int TodaySales { get; set; }
        public int TodayPurchases { get; set; }
    }

    public class SalesChartDataDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class RecentActivityDto
    {
        public string Type { get; set; } = string.Empty; // SALE, PURCHASE, INVENTORY
        public string Reference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class TopProductDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DashboardDataDto
    {
        public DashboardStatsDto Stats { get; set; } = null!;
        public List<SalesChartDataDto> SalesChartData { get; set; } = new();
        public List<StockAlertDto> StockAlerts { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
    }
}
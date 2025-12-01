namespace WilliamMetalAPI.DTOs
{
    public class InventoryMovementDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // IN, OUT, ADJUSTMENT
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public string? ReferenceType { get; set; }
        public string? ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string VariantId { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
    }

    public class StockMovementDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string VariantId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Type { get; set; } = string.Empty; // IN, OUT
        public string? Notes { get; set; }
    }

    public class StockAdjustmentDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string VariantId { get; set; } = string.Empty;
        public int NewStock { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class StockAlertDto
    {
        public string Product { get; set; } = string.Empty;
        public string Variant { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinStock { get; set; }
        public string Type { get; set; } = string.Empty; // out_of_stock, low_stock
    }

    public class InventoryStatsDto
    {
        public int TotalItems { get; set; }
        public decimal TotalValue { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
    }
}
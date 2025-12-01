namespace WilliamMetalAPI.DTOs
{
    public class PurchaseDto
    {
        public string Id { get; set; } = string.Empty;
        public string PurchaseNumber { get; set; } = string.Empty;
        public SupplierDto Supplier { get; set; } = null!;
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string DeliveryStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public List<PurchaseItemDto> Items { get; set; } = new();
    }

    public class PurchaseItemDto
    {
        public string Id { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string VariantId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
    }

    public class SupplierDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Contact { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePurchaseDto
    {
        public CreateSupplierDto Supplier { get; set; } = null!;
        public List<CreatePurchaseItemDto> Items { get; set; } = new();
        public string PaymentStatus { get; set; } = string.Empty;
        public string DeliveryStatus { get; set; } = string.Empty;
    }

    public class CreateSupplierDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Contact { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class CreatePurchaseItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string VariantId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }

    public class PurchaseFilterDto
    {
        public string? Search { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? PaymentStatus { get; set; }
        public string? DeliveryStatus { get; set; }
    }

    public class UpdatePurchaseStatusDto
    {
        public string? PaymentStatus { get; set; }
        public string? DeliveryStatus { get; set; }
    }
}
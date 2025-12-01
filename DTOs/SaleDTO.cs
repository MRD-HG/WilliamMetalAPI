namespace WilliamMetalAPI.DTOs
{
    public class SaleDto
    {
        public string Id { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public CustomerDto Customer { get; set; } = null!;
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public List<SaleItemDto> Items { get; set; } = new();
    }

    public class SaleItemDto
    {
        public string Id { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string VariantId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
    }

    public class CustomerDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSaleDto
    {
        public CreateCustomerDto Customer { get; set; } = null!;
        public List<CreateSaleItemDto> Items { get; set; } = new();
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal TaxRate { get; set; } = 10;
    }

    public class CreateCustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class CreateSaleItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string VariantId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class SaleFilterDto
    {
        public string? Search { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Status { get; set; }
    }
}
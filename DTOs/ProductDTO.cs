namespace WilliamMetalAPI.DTOs
{
    public class ProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? NameFr { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ProductVariantDto> Variants { get; set; } = new();
    }

    public class ProductVariantDto
    {
        public string Id { get; set; } = string.Empty;
        public string Specification { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int Stock { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }
        public string ProductId { get; set; } = string.Empty;
    }

    public class CreateProductDto
    {
        public string NameAr { get; set; } = string.Empty;
        public string? NameFr { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public List<CreateProductVariantDto> Variants { get; set; } = new();
    }

    public class CreateProductVariantDto
    {
        public string Specification { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int Stock { get; set; }
        public int MinStock { get; set; } = 20;
        public int MaxStock { get; set; } = 200;
    }

    public class UpdateProductDto
    {
        public string? NameAr { get; set; }
        public string? NameFr { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
    }

    public class ProductFilterDto
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? StockStatus { get; set; } // available, low, out
    }
}
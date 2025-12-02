using System.ComponentModel.DataAnnotations;

namespace WilliamMetalAPI.Models
{
    public class Sale
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;
        
        [Required]
        public decimal Subtotal { get; set; }
        
        [Required]
        public decimal Tax { get; set; }
        
        [Required]
        public decimal Total { get; set; }
        
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        
        [Required]
        public SaleStatus Status { get; set; } = SaleStatus.COMPLETED;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        
        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
        public virtual User? Creator { get; set; }
    }

    public class SaleItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        [Required]
        public decimal TotalPrice { get; set; }
        
        // Foreign keys
        [Required]
        public string SaleId { get; set; } = string.Empty;
        [Required]
        public string ProductId { get; set; } = string.Empty;
        [Required]
        public string VariantId { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Sale Sale { get; set; } = null!;
        public virtual ProductVariant Variant { get; set; } = null!;
    }

    public class Customer
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string? Phone { get; set; }
        
        [MaxLength(500)]
        public string? Address { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }

    public enum PaymentMethod
    {
        CASH,
        CREDIT,
        CHECK
    }

    public enum SaleStatus
    {
        PENDING,
        COMPLETED,
        CANCELLED
    }
}

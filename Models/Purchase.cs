using System.ComponentModel.DataAnnotations;

namespace WilliamMetalAPI.Models
{
    public class Purchase
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(50)]
        public string PurchaseNumber { get; set; } = string.Empty;
        
        [Required]
        public decimal Subtotal { get; set; }
        
        [Required]
        public decimal Tax { get; set; }
        
        [Required]
        public decimal Total { get; set; }
        
        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.PENDING;
        
        [Required]
        public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.PENDING;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        [Required]
        public string SupplierId { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        
        // Navigation properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
        public virtual User? Creator { get; set; }
    }

    public class PurchaseItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public decimal UnitCost { get; set; }
        
        [Required]
        public decimal TotalCost { get; set; }
        
        // Foreign keys
        [Required]
        public string PurchaseId { get; set; } = string.Empty;
        [Required]
        public string ProductId { get; set; } = string.Empty;
        [Required]
        public string VariantId { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Purchase Purchase { get; set; } = null!;
        public virtual ProductVariant Variant { get; set; } = null!;
    }

    public class Supplier
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? Contact { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
        
        [MaxLength(500)]
        public string? Address { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }

    public enum PaymentStatus
    {
        PENDING,
        PAID,
        PARTIAL
    }

    public enum DeliveryStatus
    {
        PENDING,
        DELIVERED,
        PARTIAL
    }
}

using System.ComponentModel.DataAnnotations;

namespace WilliamMetalAPI.Models
{
    public class Product
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(200)]
        public string NameAr { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? NameFr { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(500)]
        public string? Image { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }

    public class ProductVariant
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(100)]
        public string Specification { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string SKU { get; set; } = string.Empty;
        
        [Required]
        public decimal Price { get; set; }
        
        public decimal Cost { get; set; }
        
        [Required]
        public int Stock { get; set; }
        
        [Required]
        public int MinStock { get; set; } = 20;
        
        [Required]
        public int MaxStock { get; set; } = 200;
        
        // Foreign key
        [Required]
        public string ProductId { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}
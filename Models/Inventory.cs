using System.ComponentModel.DataAnnotations;

namespace WilliamMetalAPI.Models
{
    public class InventoryMovement
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public MovementType Type { get; set; } // IN, OUT, ADJUSTMENT
        
        [Required]
        public int Quantity { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        [MaxLength(100)]
        public string? ReferenceType { get; set; } // SALE, PURCHASE, ADJUSTMENT
        
        [MaxLength(100)]
        public string? ReferenceId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        [Required]
        public string ProductId { get; set; } = string.Empty;
        [Required]
        public string VariantId { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        
        // Navigation properties
        public virtual ProductVariant Variant { get; set; } = null!;
        public virtual User? Creator { get; set; }
    }

    public enum MovementType
    {
        IN,
        OUT,
        ADJUSTMENT
    }
}
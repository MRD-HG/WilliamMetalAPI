using System.ComponentModel.DataAnnotations;

namespace WilliamMetalAPI.Models
{
    public class CompanySettings
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = "William Metal";
        
        [MaxLength(500)]
        public string? Address { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
        
        [MaxLength(100)]
        public string? Email { get; set; }
        
        [Required]
        public decimal TaxRate { get; set; } = 10;
        
        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "MAD";
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class InventorySettings
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public int LowStockThreshold { get; set; } = 20;
        
        [Required]
        public int AutoReorderPoint { get; set; } = 50;
        
        [MaxLength(100)]
        public string? DefaultSupplier { get; set; }
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class NotificationSettings
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public bool LowStockAlert { get; set; } = true;
        
        [Required]
        public bool DailyReport { get; set; } = true;
        
        [Required]
        public bool EmailNotifications { get; set; } = true;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
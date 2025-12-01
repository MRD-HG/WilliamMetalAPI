using System.ComponentModel.DataAnnotations;

namespace WilliamMetalAPI.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? Email { get; set; }
        
        [Required]
        public UserRole Role { get; set; } = UserRole.EMPLOYEE;
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }

    public enum UserRole
    {
        ADMIN,
        MANAGER,
        EMPLOYEE
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
    }

    // Register request DTO
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public UserRole Role { get; set; } = UserRole.EMPLOYEE;
        public bool IsActive { get; set; } = true;
    }
}
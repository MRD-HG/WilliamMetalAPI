using Microsoft.EntityFrameworkCore;
using WilliamMetalAPI.Models;

namespace WilliamMetalAPI.Data
{
    public class WilliamMetalContext : DbContext
    {
        public WilliamMetalContext(DbContextOptions<WilliamMetalContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<CompanySettings> CompanySettings { get; set; }
        public DbSet<InventorySettings> InventorySettings { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Role).HasConversion<string>();
            });

            // Product configurations
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.NameAr);
                entity.HasMany(e => e.Variants)
                      .WithOne(e => e.Product)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ProductVariant configurations
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.HasIndex(e => new { e.ProductId, e.Specification }).IsUnique();
            });

            // InventoryMovement configurations
            modelBuilder.Entity<InventoryMovement>(entity =>
            {
                entity.Property(e => e.Type).HasConversion<string>();
                entity.HasOne(e => e.Variant)
                      .WithMany(e => e.InventoryMovements)
                      .HasForeignKey(e => e.VariantId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Creator)
                      .WithMany(e => e.InventoryMovements)
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Sale configurations
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.Property(e => e.PaymentMethod).HasConversion<string>();
                entity.Property(e => e.Status).HasConversion<string>();
                entity.HasOne(e => e.Creator)
                      .WithMany(e => e.Sales)
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // SaleItem configurations
            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.HasOne(e => e.Sale)
                      .WithMany(e => e.Items)
                      .HasForeignKey(e => e.SaleId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Variant)
                      .WithMany(e => e.SaleItems)
                      .HasForeignKey(e => e.VariantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Customer configurations
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.Name);
                entity.HasMany(e => e.Sales)
                      .WithOne(e => e.Customer)
                      .HasForeignKey(e => e.Id)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Purchase configurations
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.Property(e => e.PaymentStatus).HasConversion<string>();
                entity.Property(e => e.DeliveryStatus).HasConversion<string>();
                entity.HasOne(e => e.Creator)
                      .WithMany(e => e.Purchases)
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // PurchaseItem configurations
            modelBuilder.Entity<PurchaseItem>(entity =>
            {
                entity.HasOne(e => e.Purchase)
                      .WithMany(e => e.Items)
                      .HasForeignKey(e => e.PurchaseId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Variant)
                      .WithMany(e => e.PurchaseItems)
                      .HasForeignKey(e => e.VariantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Supplier configurations
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasIndex(e => e.Name);
                entity.HasMany(e => e.Purchases)
                      .WithOne(e => e.Supplier)
                      .HasForeignKey(e => e.Id)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Settings configurations
            modelBuilder.Entity<CompanySettings>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });
        }
    }
}
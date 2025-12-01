using WilliamMetalAPI.Models;

namespace WilliamMetalAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(WilliamMetalContext context)
        {
            context.Database.EnsureCreated();

            // Check if database is already seeded
            if (context.Users.Any())
            {
                return; // DB has been seeded
            }

            // Seed default admin user
            var adminUser = new User
            {
                Username = "admin",
                FullName = "مدير النظام",
                Email = "admin@williammetal.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = UserRole.ADMIN,
                IsActive = true
            };

            context.Users.Add(adminUser);
            context.SaveChanges();

            // Seed company settings
            var companySettings = new CompanySettings
            {
                Name = "William Metal",
                Address = "قلعة مكونـــة، المغرب",
                Phone = "+212522123456",
                Email = "info@williammetal.com",
                TaxRate = 20,
                Currency = "MAD"
            };

            context.CompanySettings.Add(companySettings);

            // Seed inventory settings
            var inventorySettings = new InventorySettings
            {
                LowStockThreshold = 20,
                AutoReorderPoint = 50
            };

            context.InventorySettings.Add(inventorySettings);

            // Seed notification settings
            var notificationSettings = new NotificationSettings
            {
                LowStockAlert = true,
                DailyReport = true,
                EmailNotifications = true
            };

            context.NotificationSettings.Add(notificationSettings);

            // Seed sample products from the JSON catalog
            SeedSampleProducts(context);

            context.SaveChanges();
        }

        private static void SeedSampleProducts(WilliamMetalContext context)
        {
            var products = new List<Product>
            {
                new Product
                {
                    NameAr = "تيب روند مويان 1.5",
                    NameFr = "Tube Rond Moyen 1.5",
                    Category = "أنابيب دائرية Tubes Round",
                    Description = "أنبوب دائري متوسط السمك 1.5 ملم",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "1.5/16",
                            SKU = "WM-RT15-16",
                            Price = 25.50m,
                            Cost = 18.00m,
                            Stock = 150,
                            MinStock = 20,
                            MaxStock = 200
                        },
                        new ProductVariant
                        {
                            Specification = "1.5/20",
                            SKU = "WM-RT15-20",
                            Price = 28.75m,
                            Cost = 20.00m,
                            Stock = 120,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                },
                new Product
                {
                    NameAr = "تيب كاري مويان 1",
                    NameFr = "Tube Carré Moyen 1",
                    Category = "أنابيب مربعة Tube Carré",
                    Description = "أنبوب مربع متوسط السمك 1 ملم",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "1/16",
                            SKU = "WM-ST1-16",
                            Price = 32.00m,
                            Cost = 24.00m,
                            Stock = 85,
                            MinStock = 15,
                            MaxStock = 150
                        },
                        new ProductVariant
                        {
                            Specification = "1/20",
                            SKU = "WM-ST1-20",
                            Price = 35.50m,
                            Cost = 26.50m,
                            Stock = 95,
                            MinStock = 15,
                            MaxStock = 150
                        }
                    }
                },
                new Product
                {
                    NameAr = "كورنير عادي",
                    NameFr = "Cornière simple",
                    Category = "زوايا Cornière",
                    Description = "زاوية حديد عادية",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "20",
                            SKU = "WM-RA-20",
                            Price = 15.00m,
                            Cost = 10.00m,
                            Stock = 200,
                            MinStock = 30,
                            MaxStock = 300
                        },
                        new ProductVariant
                        {
                            Specification = "25",
                            SKU = "WM-RA-25",
                            Price = 18.50m,
                            Cost = 12.50m,
                            Stock = 180,
                            MinStock = 30,
                            MaxStock = 300
                        }
                    }
                },
                new Product
                {
                    NameAr = "فير بلا",
                    NameFr = "Fer Plat",
                    Category = "قضبان Plat",
                    Description = "قضيب حديد مسطح",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "10/4",
                            SKU = "WM-FB-10-4",
                            Price = 12.00m,
                            Cost = 8.00m,
                            Stock = 300,
                            MinStock = 50,
                            MaxStock = 400
                        },
                        new ProductVariant
                        {
                            Specification = "20/3",
                            SKU = "WM-FB-20-3",
                            Price = 22.00m,
                            Cost = 16.00m,
                            Stock = 250,
                            MinStock = 50,
                            MaxStock = 400
                        }
                    }
                }
            };

            context.Products.AddRange(products);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using WilliamMetalAPI.Models;

namespace WilliamMetalAPI.Data
{
    public static class DbInitializer
    {
        /// <summary>
        /// Seed safe defaults. This method is idempotent:
        /// - It will create missing settings/users.
        /// - It will only add products/variants that don't already exist (by SKU).
        /// </summary>
        public static void Initialize(WilliamMetalContext context)
        {
            // IMPORTANT:
            // Do NOT call EnsureCreated when you are using migrations.
            // Migrations are applied in Program.cs via context.Database.Migrate().

            SeedAdminUser(context);
            SeedCompanySettings(context);
            SeedInventorySettings(context);
            SeedNotificationSettings(context);
            SeedProducts(context);

            context.SaveChanges();
        }

        private static void SeedAdminUser(WilliamMetalContext context)
        {
            if (context.Users.Any()) return;

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
        }

        private static void SeedCompanySettings(WilliamMetalContext context)
        {
            if (context.CompanySettings.Any()) return;

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
        }

        private static void SeedInventorySettings(WilliamMetalContext context)
        {
            if (context.InventorySettings.Any()) return;

            var inventorySettings = new InventorySettings
            {
                LowStockThreshold = 20,
                AutoReorderPoint = 50
            };

            context.InventorySettings.Add(inventorySettings);
        }

        private static void SeedNotificationSettings(WilliamMetalContext context)
        {
            if (context.NotificationSettings.Any()) return;

            var notificationSettings = new NotificationSettings
            {
                LowStockAlert = true,
                DailyReport = true,
                EmailNotifications = true
            };

            context.NotificationSettings.Add(notificationSettings);
        }

        private static void SeedProducts(WilliamMetalContext context)
        {
            // Add-only seeding (no drop, no overwrite): skip any variant whose SKU already exists.
            var existingSkus = context.ProductVariants
                .AsNoTracking()
                .Select(v => v.SKU)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var seed = BuildSeedProducts();
            var toAdd = new List<Product>();

            foreach (var p in seed)
            {
                var variantsToAdd = p.Variants
                    .Where(v => !existingSkus.Contains(v.SKU))
                    .ToList();

                if (!variantsToAdd.Any())
                    continue;

                // Keep only the non-existing variants
                p.Variants = variantsToAdd;
                toAdd.Add(p);

                foreach (var v in variantsToAdd)
                    existingSkus.Add(v.SKU);
            }

            if (toAdd.Any())
                context.Products.AddRange(toAdd);
        }

        /// <summary>
        /// Catalog: one variant per product (you will manage the rest from the app).
        /// Categories are bilingual (Arabic + French).
        /// </summary>
        private static List<Product> BuildSeedProducts()
        {
            // Category naming convention: "Arabic | Français"
            const string CAT_CORNIERE = "زوايا | Cornière";
            const string CAT_TUBE_ROND = "أنابيب دائرية | Tubes Rond";
            const string CAT_TUBE_CARRE = "أنابيب مربعة | Tubes Carré";
            const string CAT_PLAT = "قضبان مسطحة | Fer Plat";
            const string CAT_GRILLAGE = "شبكات | Grillage";
            const string CAT_TOLE = "طولات | Tôle";
            const string CAT_AUTRES = "أخرى | Autres";

            return new List<Product>
            {
                // ====== CORNIÈRE / زوايا ======
                new Product
                {
                    NameAr = "كورنير عادي",
                    NameFr = "Cornière simple",
                    Category = CAT_CORNIERE,
                    Description = "زاوية حديد عادية",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "20x20x3",
                            SKU = "WM-CO-S-20",
                            Price = 0m,
                            Cost = 0m,
                            Stock = 0,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                },
                new Product
                {
                    NameAr = "كورنير مويان 2.8",
                    NameFr = "Cornière moyenne 2.8",
                    Category = CAT_CORNIERE,
                    Description = "زاوية حديد متوسطة السماكة 2.8 مم",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "30x30x2.8",
                            SKU = "WM-CO-M28-30",
                            Price = 0m,
                            Cost = 0m,
                            Stock = 0,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                },

                // ====== TUBE ROND / تيب روند ======
                new Product
                {
                    NameAr = "تيب روند 1",
                    NameFr = "Tube Rond 1",
                    Category = CAT_TUBE_ROND,
                    Description = "أنبوب دائري سماكة 1 مم",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "1/16",
                            SKU = "WM-TR1-16",
                            Price = 0m,
                            Cost = 0m,
                            Stock = 0,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                },
                new Product
                {
                    NameAr = "تيب روند مويان 1.5",
                    NameFr = "Tube Rond Moyen 1.5",
                    Category = CAT_TUBE_ROND,
                    Description = "أنبوب دائري متوسط السماكة 1.5 مم",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "1.5/20",
                            SKU = "WM-TR15-20",
                            Price = 0m,
                            Cost = 0m,
                            Stock = 0,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                },
                new Product
                {
                    NameAr = "تيب روند غليظ 2",
                    NameFr = "Tube Rond épais 2",
                    Category = CAT_TUBE_ROND,
                    Description = "أنبوب دائري سميك 2 مم",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "2/30",
                            SKU = "WM-TR2-30",
                            Price = 0m,
                            Cost = 0m,
                            Stock = 0,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                },
                new Product
                {
                    NameAr = "تيب روند غليظ 3",
                    NameFr = "Tube Rond épais 3",
                    Category = CAT_TUBE_ROND,
                    Description = "تيب روند غليظ سماكة 3 مم",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "3/10",
                            SKU = "WM-TR3-10",
                            Price = 0m,
                            Cost = 0m,
                            Stock = 0,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                },

                // ====== TUBE CARRÉ / تيب كاري ======
                new Product
                {
                    NameAr = "تيب كاري عادية",
                    NameFr = "Tube Carré simple",
                    Category = CAT_TUBE_CARRE,
                    Description = "أنبوب مربع عادي",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "20x20x1",
                            SKU = "WM-TC-S-20",
                            Price = 0m,
                            Cost = 0m,
                            Stock = 0,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                },
                new Product
                {
                    NameAr = "تيب كاري مويان 1",
                    NameFr = "Tube Carré Moyen 1",
                    Category = CAT_TUBE_CARRE,
                    Description = "أنبوب مربع متوسط السماكة 1 مم",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "30x30x1",
                            SKU = "WM-TC-M1-30",
                            Price = 0m,
                            Cost = 0m,
                            Stock = 0,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                },

                // ====== FER PLAT / فير بلا ======
                new Product
                {
                    NameAr = "فير بلا",
                    NameFr = "Fer Plat",
                    Category = CAT_PLAT,
                    Description = "قضيب حديد مسطح",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "20x3",
                            SKU = "WM-FP-20x3",
                            Price = 0m,
                            Cost = 0m,
                            Stock = 0,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                },

                // ====== GRILLAGE / شبكة جرادي ======
                new Product
                {
                    NameAr = "شبكة جرادي",
                    NameFr = "Grillage",
                    Category = CAT_GRILLAGE,
                    Description = "شبكة جرادي حديد",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "1m",
                            SKU = "WM-GR-1M",
                            Price = 0m,
                            Cost = 0m,
                            Stock = 0,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                },

                // ====== TÔLE / طولات ======
                new Product
                {
                    NameAr = "طولة سامبل عادية",
                    NameFr = "Tôle simple",
                    Category = CAT_TOLE,
                    Description = "طولة سامبل عادية",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "5 كحلة",
                            SKU = "WM-TS-5B",
                            Price = 0m,
                            Cost = 0m,
                            Stock = 0,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                },
                new Product
                {
                    NameAr = "طولة ريدال 2 متر 13",
                    NameFr = "Tôle Redal 2m (ép.13)",
                    Category = CAT_TOLE,
                    Description = "طولة ريدال طول 2 متر سماكة 13",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "55/2",
                            SKU = "WM-TRD2-13-55",
                            Price = 0m,
                            Cost = 0m,
                            Stock = 0,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                },

                // ====== AUTRES / أخرى ======
                new Product
                {
                    NameAr = "أخرى",
                    NameFr = "Autres",
                    Category = CAT_AUTRES,
                    Description = "منتجات متفرقة غير مصنفة",
                    Image = "",
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Specification = "Divers",
                            SKU = "WM-OT-001",
                            Price = 0m,
                            Cost = 0m,
                            Stock = 0,
                            MinStock = 20,
                            MaxStock = 200
                        }
                    }
                }
            };
        }
    }
}

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WilliamMetalAPI.Data;
using WilliamMetalAPI.DTOs;
using WilliamMetalAPI.Models;

namespace WilliamMetalAPI.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly WilliamMetalContext _context;
        private readonly IMapper _mapper;

        public InventoryService(WilliamMetalContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<InventoryStatsDto> GetInventoryStatsAsync()
        {
            var allVariants = await _context.ProductVariants
                .Include(v => v.Product)
                .ToListAsync();

            var stats = new InventoryStatsDto
            {
                TotalItems = allVariants.Count,
                TotalValue = allVariants.Sum(v => v.Stock * v.Cost),
                LowStockItems = allVariants.Count(v => v.Stock > 0 && v.Stock <= v.MinStock),
                OutOfStockItems = allVariants.Count(v => v.Stock == 0)
            };

            return stats;
        }

        public async Task<List<InventoryMovementDto>> GetInventoryMovementsAsync(string? productId = null, string? variantId = null)
        {
            var query = _context.InventoryMovements
                .Include(m => m.Variant)
                .ThenInclude(v => v.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(productId))
            {
                query = query.Where(m => m.ProductId == productId);
            }

            if (!string.IsNullOrEmpty(variantId))
            {
                query = query.Where(m => m.VariantId == variantId);
            }

            var movements = await query
                .OrderByDescending(m => m.CreatedAt)
                .Take(100)
                .ToListAsync();

            return movements.Select(m => new InventoryMovementDto
            {
                Id = m.Id,
                Type = m.Type.ToString(),
                Quantity = m.Quantity,
                Notes = m.Notes,
                ReferenceType = m.ReferenceType,
                ReferenceId = m.ReferenceId,
                CreatedAt = m.CreatedAt,
                ProductId = m.ProductId,
                VariantId = m.VariantId,
                CreatedBy = m.CreatedBy,
                ProductName = m.Variant.Product.NameAr,
                VariantName = m.Variant.Specification
            }).ToList();
        }

        public async Task<List<StockAlertDto>> GetStockAlertsAsync()
        {
            var alerts = new List<StockAlertDto>();

            var variants = await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.Stock <= v.MinStock)
                .ToListAsync();

            foreach (var variant in variants)
            {
                alerts.Add(new StockAlertDto
                {
                    Product = variant.Product.NameAr,
                    Variant = variant.Specification,
                    CurrentStock = variant.Stock,
                    MinStock = variant.MinStock,
                    Type = variant.Stock == 0 ? "out_of_stock" : "low_stock"
                });
            }

            return alerts.OrderBy(a => a.Type).ThenBy(a => a.CurrentStock).ToList();
        }

        public async Task<bool> UpdateStockAsync(StockMovementDto movement, string? userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == movement.VariantId && v.ProductId == movement.ProductId);

                if (variant == null)
                    return false;

                // Update stock based on movement type
                if (movement.Type == "IN")
                {
                    variant.Stock += movement.Quantity;
                }
                else if (movement.Type == "OUT")
                {
                    if (variant.Stock < movement.Quantity)
                        return false; // Insufficient stock
                    
                    variant.Stock -= movement.Quantity;
                }

                // Create inventory movement record
                var createdBy = string.IsNullOrWhiteSpace(userId) ? null : userId;

                var inventoryMovement = new InventoryMovement
                {
                    ProductId = movement.ProductId,
                    VariantId = movement.VariantId,
                    Type = Enum.Parse<MovementType>(movement.Type),
                    Quantity = movement.Quantity,
                    Notes = movement.Notes,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryMovements.Add(inventoryMovement);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> AdjustStockAsync(StockAdjustmentDto adjustment, string? userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == adjustment.VariantId && v.ProductId == adjustment.ProductId);

                if (variant == null)
                    return false;

                var oldStock = variant.Stock;
                var newStock = adjustment.NewStock;
                var difference = newStock - oldStock;

                if (difference == 0)
                    return true; // No change needed

                // Update stock
                variant.Stock = newStock;

                // Create inventory movement record
                var createdBy = string.IsNullOrWhiteSpace(userId) ? null : userId;

                var inventoryMovement = new InventoryMovement
                {
                    ProductId = adjustment.ProductId,
                    VariantId = adjustment.VariantId,
                    Type = MovementType.ADJUSTMENT,
                    Quantity = Math.Abs(difference),
                    Notes = adjustment.Reason,
                    ReferenceType = "ADJUSTMENT",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryMovements.Add(inventoryMovement);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<InventoryMovementDto?> GetMovementByIdAsync(string id)
        {
            var movement = await _context.InventoryMovements
                .Include(m => m.Variant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movement == null)
                return null;

            return new InventoryMovementDto
            {
                Id = movement.Id,
                Type = movement.Type.ToString(),
                Quantity = movement.Quantity,
                Notes = movement.Notes,
                ReferenceType = movement.ReferenceType,
                ReferenceId = movement.ReferenceId,
                CreatedAt = movement.CreatedAt,
                ProductId = movement.ProductId,
                VariantId = movement.VariantId,
                CreatedBy = movement.CreatedBy,
                ProductName = movement.Variant.Product.NameAr,
                VariantName = movement.Variant.Specification
            };
        }
    }
}

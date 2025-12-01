using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WilliamMetalAPI.Data;
using WilliamMetalAPI.DTOs;
using WilliamMetalAPI.Models;

namespace WilliamMetalAPI.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly WilliamMetalContext _context;
        private readonly IMapper _mapper;

        public PurchaseService(WilliamMetalContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<PurchaseDto>> GetPurchasesAsync(PurchaseFilterDto filter)
        {
            var query = _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                .ThenInclude(i => i.Variant)
                .ThenInclude(v => v.Product)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var searchTerm = filter.Search.ToLower();
                query = query.Where(p => 
                    p.PurchaseNumber.ToLower().Contains(searchTerm) ||
                    p.Supplier.Name.ToLower().Contains(searchTerm));
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= filter.DateTo.Value);
            }

            if (!string.IsNullOrEmpty(filter.PaymentStatus))
            {
                query = query.Where(p => p.PaymentStatus.ToString() == filter.PaymentStatus);
            }

            if (!string.IsNullOrEmpty(filter.DeliveryStatus))
            {
                query = query.Where(p => p.DeliveryStatus.ToString() == filter.DeliveryStatus);
            }

            var purchases = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<PurchaseDto>>(purchases);
        }

        public async Task<PurchaseDto?> GetPurchaseByIdAsync(string id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                .ThenInclude(i => i.Variant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            return purchase == null ? null : _mapper.Map<PurchaseDto>(purchase);
        }

        public async Task<PurchaseDto> CreatePurchaseAsync(CreatePurchaseDto createDto, string? userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Create or get supplier
                var supplier = new Supplier
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = createDto.Supplier.Name,
                    Contact = createDto.Supplier.Contact,
                    Phone = createDto.Supplier.Phone,
                    Address = createDto.Supplier.Address
                };

                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();

                // Calculate totals
                var subtotal = createDto.Items.Sum(i => i.UnitCost * i.Quantity);
                var tax = subtotal * 0.1m; // 10% tax
                var total = subtotal + tax;

                // Create purchase
                var purchase = new Purchase
                {
                    Id = Guid.NewGuid().ToString(),
                    PurchaseNumber = await GeneratePurchaseNumberAsync(),
                    Supplier = supplier,
                    Subtotal = subtotal,
                    Tax = tax,
                    Total = total,
                    PaymentStatus = Enum.Parse<PaymentStatus>(createDto.PaymentStatus),
                    DeliveryStatus = Enum.Parse<DeliveryStatus>(createDto.DeliveryStatus),
                    CreatedBy = userId
                };

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

                // Create purchase items and update inventory
                foreach (var itemDto in createDto.Items)
                {
                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(v => v.Id == itemDto.VariantId && v.ProductId == itemDto.ProductId);

                    if (variant == null)
                    {
                        throw new InvalidOperationException($"Variant {itemDto.VariantId} not found");
                    }

                    // Create purchase item
                    var purchaseItem = new PurchaseItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        PurchaseId = purchase.Id,
                        ProductId = itemDto.ProductId,
                        VariantId = itemDto.VariantId,
                        Quantity = itemDto.Quantity,
                        UnitCost = itemDto.UnitCost,
                        TotalCost = itemDto.UnitCost * itemDto.Quantity
                    };

                    _context.PurchaseItems.Add(purchaseItem);

                    // Update stock (only if delivered)
                    if (purchase.DeliveryStatus == DeliveryStatus.DELIVERED || 
                        purchase.DeliveryStatus == DeliveryStatus.PARTIAL)
                    {
                        variant.Stock += itemDto.Quantity;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<PurchaseDto>(purchase);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdatePurchaseStatusAsync(string id, UpdatePurchaseStatusDto statusDto)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
                return false;

            if (!string.IsNullOrEmpty(statusDto.PaymentStatus))
            {
                purchase.PaymentStatus = Enum.Parse<PaymentStatus>(statusDto.PaymentStatus);
            }

            if (!string.IsNullOrEmpty(statusDto.DeliveryStatus))
            {
                var oldStatus = purchase.DeliveryStatus;
                purchase.DeliveryStatus = Enum.Parse<DeliveryStatus>(statusDto.DeliveryStatus);

                // Update stock if status changed to delivered
                if (oldStatus != DeliveryStatus.DELIVERED && purchase.DeliveryStatus == DeliveryStatus.DELIVERED)
                {
                    var items = await _context.PurchaseItems
                        .Where(i => i.PurchaseId == id)
                        .ToListAsync();

                    foreach (var item in items)
                    {
                        var variant = await _context.ProductVariants
                            .FirstOrDefaultAsync(v => v.Id == item.VariantId);

                        if (variant != null)
                        {
                            variant.Stock += item.Quantity;
                        }
                    }
                }
            }

            purchase.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePurchaseAsync(string id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
                return false;

            // Restore stock if items were delivered
            if (purchase.DeliveryStatus == DeliveryStatus.DELIVERED || 
                purchase.DeliveryStatus == DeliveryStatus.PARTIAL)
            {
                foreach (var item in purchase.Items)
                {
                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(v => v.Id == item.VariantId);

                    if (variant != null)
                    {
                        variant.Stock -= item.Quantity;
                    }
                }
            }

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SupplierDto>> GetSuppliersAsync()
        {
            var suppliers = await _context.Suppliers
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<SupplierDto>>(suppliers);
        }

        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto supplierDto)
        {
            var supplier = _mapper.Map<Supplier>(supplierDto);
            supplier.Id = Guid.NewGuid().ToString();

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task<string> GeneratePurchaseNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _context.Purchases.CountAsync(p => p.CreatedAt.Year == year);
            return $"PO-{year}-{++count:0000}";
        }
    }
}
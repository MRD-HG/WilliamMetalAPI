using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WilliamMetalAPI.Data;
using WilliamMetalAPI.DTOs;
using WilliamMetalAPI.Models;

namespace WilliamMetalAPI.Services
{
    public class SaleService : ISaleService
    {
        private readonly WilliamMetalContext _context;
        private readonly IMapper _mapper;

        public SaleService(WilliamMetalContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<SaleDto>> GetSalesAsync(SaleFilterDto filter)
        {
            var query = _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Items)
                .ThenInclude(i => i.Variant)
                .ThenInclude(v => v.Product)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var searchTerm = filter.Search.ToLower();
                query = query.Where(s => 
                    s.InvoiceNumber.ToLower().Contains(searchTerm) ||
                    s.Customer.Name.ToLower().Contains(searchTerm));
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(s => s.CreatedAt >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(s => s.CreatedAt <= filter.DateTo.Value);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(s => s.Status.ToString() == filter.Status);
            }

            var sales = await query
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<SaleDto>>(sales);
        }

        public async Task<SaleDto?> GetSaleByIdAsync(string id)
        {
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Items)
                .ThenInclude(i => i.Variant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            return sale == null ? null : _mapper.Map<SaleDto>(sale);
        }

        public async Task<SaleDto> CreateSaleAsync(CreateSaleDto createDto, string? userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var createdBy = string.IsNullOrWhiteSpace(userId) ? null : userId;

                // Create or get customer
                var customer = new Customer
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = createDto.Customer.Name,
                    Phone = createDto.Customer.Phone,
                    Address = createDto.Customer.Address
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Calculate totals
                var subtotal = createDto.Items.Sum(i => i.UnitPrice * i.Quantity);
                var tax = subtotal * (createDto.TaxRate / 100);
                var total = subtotal + tax;

                // Create sale
                var sale = new Sale
                {
                    Id = Guid.NewGuid().ToString(),
                    InvoiceNumber = await GenerateInvoiceNumberAsync(),
                    Customer = customer,
                    CustomerId = customer.Id,
                    Subtotal = subtotal,
                    Tax = tax,
                    Total = total,
                    PaymentMethod = Enum.Parse<PaymentMethod>(createDto.PaymentMethod),
                    Status = SaleStatus.COMPLETED,
                    CreatedBy = createdBy
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                // Create sale items and update inventory
                foreach (var itemDto in createDto.Items)
                {
                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(v => v.Id == itemDto.VariantId && v.ProductId == itemDto.ProductId);

                    if (variant == null || variant.Stock < itemDto.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for variant {itemDto.VariantId}");
                    }

                    // Create sale item
                    var saleItem = new SaleItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        SaleId = sale.Id,
                        ProductId = itemDto.ProductId,
                        VariantId = itemDto.VariantId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        TotalPrice = itemDto.UnitPrice * itemDto.Quantity
                    };

                    _context.SaleItems.Add(saleItem);

                    // Update stock
                    variant.Stock -= itemDto.Quantity;

                    // Create inventory movement
                    var inventoryMovement = new InventoryMovement
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = itemDto.ProductId,
                        VariantId = itemDto.VariantId,
                        Type = MovementType.OUT,
                        Quantity = itemDto.Quantity,
                        ReferenceType = "SALE",
                        ReferenceId = sale.Id,
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.InventoryMovements.Add(inventoryMovement);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<SaleDto>(sale);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateSaleStatusAsync(string id, string status)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
                return false;

            sale.Status = Enum.Parse<SaleStatus>(status);
            sale.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSaleAsync(string id)
        {
            var sale = await _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
                return false;

            // Restore stock for cancelled sales
            if (sale.Status == SaleStatus.COMPLETED)
            {
                foreach (var item in sale.Items)
                {
                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(v => v.Id == item.VariantId);

                    if (variant != null)
                    {
                        variant.Stock += item.Quantity;
                    }
                }
            }

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CustomerDto>> GetCustomersAsync()
        {
            var customers = await _context.Customers
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<CustomerDto>>(customers);
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto customerDto)
        {
            var customer = _mapper.Map<Customer>(customerDto);
            customer.Id = Guid.NewGuid().ToString();

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _context.Sales.CountAsync(s => s.CreatedAt.Year == year);
            return $"INV-{year}-{++count:0000}";
        }
    }
}

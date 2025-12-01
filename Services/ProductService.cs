using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WilliamMetalAPI.Data;
using WilliamMetalAPI.DTOs;
using WilliamMetalAPI.Models;

namespace WilliamMetalAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly WilliamMetalContext _context;
        private readonly IMapper _mapper;

        public ProductService(WilliamMetalContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ProductDto>> GetProductsAsync(ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Variants)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var searchTerm = filter.Search.ToLower();
                query = query.Where(p => 
                    p.NameAr.ToLower().Contains(searchTerm) ||
                    p.NameFr.ToLower().Contains(searchTerm) ||
                    p.Category.ToLower().Contains(searchTerm));
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(filter.Category))
            {
                query = query.Where(p => p.Category == filter.Category);
            }

            // Apply stock status filter
            if (!string.IsNullOrEmpty(filter.StockStatus))
            {
                switch (filter.StockStatus.ToLower())
                {
                    case "available":
                        query = query.Where(p => p.Variants.All(v => v.Stock > v.MinStock));
                        break;
                    case "low":
                        query = query.Where(p => p.Variants.Any(v => v.Stock > 0 && v.Stock <= v.MinStock));
                        break;
                    case "out":
                        query = query.Where(p => p.Variants.Any(v => v.Stock == 0));
                        break;
                }
            }

            var products = await query.OrderBy(p => p.NameAr).ToListAsync();
            return _mapper.Map<List<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetProductByIdAsync(string id)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null ? null : _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto, string? userId)
        {
            var product = _mapper.Map<Product>(createDto);
            product.Id = Guid.NewGuid().ToString();

            // Generate SKUs if not provided
            foreach (var variant in product.Variants)
            {
                if (string.IsNullOrEmpty(variant.SKU))
                {
                    variant.SKU = GenerateSKU(product.NameAr, variant.Specification);
                }
                variant.Id = Guid.NewGuid().ToString();
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto?> UpdateProductAsync(string id, UpdateProductDto updateDto)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return null;

            _mapper.Map(updateDto, product);
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<ProductDto>> SearchProductsAsync(string query)
        {
            var searchTerm = query.ToLower();
            var products = await _context.Products
                .Include(p => p.Variants)
                .Where(p => 
                    p.NameAr.ToLower().Contains(searchTerm) ||
                    p.NameFr.ToLower().Contains(searchTerm) ||
                    p.Variants.Any(v => 
                        v.Specification.ToLower().Contains(searchTerm) ||
                        v.SKU.ToLower().Contains(searchTerm)))
                .OrderBy(p => p.NameAr)
                .ToListAsync();

            return _mapper.Map<List<ProductDto>>(products);
        }

        public async Task<ProductVariantDto> AddVariantAsync(string productId, CreateProductVariantDto variantDto)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new ArgumentException("Product not found");

            var variant = _mapper.Map<ProductVariant>(variantDto);
            variant.Id = Guid.NewGuid().ToString();
            variant.ProductId = productId;

            if (string.IsNullOrEmpty(variant.SKU))
            {
                variant.SKU = GenerateSKU(product.NameAr, variant.Specification);
            }

            product.Variants.Add(variant);
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<ProductVariantDto>(variant);
        }

        public async Task<ProductVariantDto?> UpdateVariantAsync(string productId, string variantId, ProductVariantDto variantDto)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId);

            if (variant == null)
                return null;

            _mapper.Map(variantDto, variant);
            
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<ProductVariantDto>(variant);
        }

        public async Task<bool> DeleteVariantAsync(string productId, string variantId)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId);

            if (variant == null)
                return false;

            _context.ProductVariants.Remove(variant);
            
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateSKU(string productName, string specification)
        {
            var productCode = new string(productName.Where(c => char.IsLetter(c)).Take(3).ToArray()).ToUpper();
            var specCode = new string(specification.Where(c => char.IsLetterOrDigit(c)).Take(5).ToArray());
            return $"WM-{productCode}-{specCode}-{DateTime.Now.Ticks % 10000}";
        }
    }
}
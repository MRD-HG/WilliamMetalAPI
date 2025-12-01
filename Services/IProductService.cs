using WilliamMetalAPI.DTOs;

namespace WilliamMetalAPI.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetProductsAsync(ProductFilterDto filter);
        Task<ProductDto?> GetProductByIdAsync(string id);
        Task<ProductDto> CreateProductAsync(CreateProductDto createDto, string? userId);
        Task<ProductDto?> UpdateProductAsync(string id, UpdateProductDto updateDto);
        Task<bool> DeleteProductAsync(string id);
        Task<List<string>> GetCategoriesAsync();
        Task<List<ProductDto>> SearchProductsAsync(string query);
        Task<ProductVariantDto> AddVariantAsync(string productId, CreateProductVariantDto variantDto);
        Task<ProductVariantDto?> UpdateVariantAsync(string productId, string variantId, ProductVariantDto variantDto);
        Task<bool> DeleteVariantAsync(string productId, string variantId);
    }
}
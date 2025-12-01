using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WilliamMetalAPI.Data;
using WilliamMetalAPI.DTOs;
using WilliamMetalAPI.Models;
using WilliamMetalAPI.Services;

namespace WilliamMetalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly WilliamMetalContext _context;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;

        public ProductsController(WilliamMetalContext context, IMapper mapper, IProductService productService)
        {
            _context = context;
            _mapper = mapper;
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter)
        {
            try
            {
                var products = await _productService.GetProductsAsync(filter);
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving products" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { success = false, message = "Product not found" });
                }

                return Ok(new { success = true, data = product });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving product" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                var product = await _productService.CreateProductAsync(createDto, userId);

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, 
                    new { success = true, data = product, message = "Product created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error creating product" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductDto updateDto)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, updateDto);
                if (product == null)
                {
                    return NotFound(new { success = false, message = "Product not found" });
                }

                return Ok(new { success = true, data = product, message = "Product updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating product" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Product not found" });
                }

                return Ok(new { success = true, message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting product" });
            }
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _productService.GetCategoriesAsync();
                return Ok(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving categories" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string query)
        {
            try
            {
                var products = await _productService.SearchProductsAsync(query);
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error searching products" });
            }
        }

        [HttpPost("{productId}/variants")]
        public async Task<IActionResult> AddVariant(string productId, [FromBody] CreateProductVariantDto variantDto)
        {
            try
            {
                var variant = await _productService.AddVariantAsync(productId, variantDto);
                return Ok(new { success = true, data = variant, message = "Variant added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error adding variant" });
            }
        }

        [HttpPut("{productId}/variants/{variantId}")]
        public async Task<IActionResult> UpdateVariant(string productId, string variantId, [FromBody] ProductVariantDto variantDto)
        {
            try
            {
                var variant = await _productService.UpdateVariantAsync(productId, variantId, variantDto);
                if (variant == null)
                {
                    return NotFound(new { success = false, message = "Variant not found" });
                }

                return Ok(new { success = true, data = variant, message = "Variant updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating variant" });
            }
        }

        [HttpDelete("{productId}/variants/{variantId}")]
        public async Task<IActionResult> DeleteVariant(string productId, string variantId)
        {
            try
            {
                var result = await _productService.DeleteVariantAsync(productId, variantId);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Variant not found" });
                }

                return Ok(new { success = true, message = "Variant deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting variant" });
            }
        }
    }
}
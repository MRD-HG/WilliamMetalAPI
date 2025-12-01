using AutoMapper;
using WilliamMetalAPI.DTOs;
using WilliamMetalAPI.Models;

namespace WilliamMetalAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();

            // Product mappings
            CreateMap<Product, ProductDto>();
            CreateMap<ProductVariant, ProductVariantDto>();
            CreateMap<CreateProductDto, Product>();
            CreateMap<CreateProductVariantDto, ProductVariant>();
            CreateMap<UpdateProductDto, Product>();

            // Inventory mappings
            CreateMap<InventoryMovement, InventoryMovementDto>();

            // Customer mappings
            CreateMap<Customer, CustomerDto>();
            CreateMap<CreateCustomerDto, Customer>();

            // Sale mappings
            CreateMap<Sale, SaleDto>();
            CreateMap<SaleItem, SaleItemDto>();
            CreateMap<CreateSaleDto, Sale>();
            CreateMap<CreateSaleItemDto, SaleItem>();

            // Supplier mappings
            CreateMap<Supplier, SupplierDto>();
            CreateMap<CreateSupplierDto, Supplier>();

            // Purchase mappings
            CreateMap<Purchase, PurchaseDto>();
            CreateMap<PurchaseItem, PurchaseItemDto>();
            CreateMap<CreatePurchaseDto, Purchase>();
            CreateMap<CreatePurchaseItemDto, PurchaseItem>();
        }
    }
}
using AutoMapper;
using WilliamMetalAPI.DTOs;
using WilliamMetalAPI.Models;

namespace WilliamMetalAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Products
            CreateMap<Product, ProductDto>();
            CreateMap<ProductVariant, ProductVariantDto>();

            // Inventory
            CreateMap<InventoryMovement, InventoryMovementDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Variant.Product.NameAr))
                .ForMember(d => d.VariantName, o => o.MapFrom(s => s.Variant.Specification));

            // Sales
            CreateMap<Customer, CustomerDto>();
            CreateMap<SaleItem, SaleItemDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Variant.Product.NameAr))
                .ForMember(d => d.VariantName, o => o.MapFrom(s => s.Variant.Specification));
            CreateMap<Sale, SaleDto>()
                .ForMember(d => d.PaymentMethod, o => o.MapFrom(s => s.PaymentMethod.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            // Purchases
            CreateMap<Supplier, SupplierDto>();
            CreateMap<PurchaseItem, PurchaseItemDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Variant.Product.NameAr))
                .ForMember(d => d.VariantName, o => o.MapFrom(s => s.Variant.Specification));
            CreateMap<Purchase, PurchaseDto>()
                .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.PaymentStatus.ToString()))
                .ForMember(d => d.DeliveryStatus, o => o.MapFrom(s => s.DeliveryStatus.ToString()));

            // Users
            CreateMap<User, UserDto>();
        }
    }
}

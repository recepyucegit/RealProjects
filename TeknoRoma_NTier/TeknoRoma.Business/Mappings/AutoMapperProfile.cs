using AutoMapper;
using TeknoRoma.Business.DTOs;
using TeknoRoma.Entities;

namespace TeknoRoma.Business.Mappings;

/// <summary>
/// AutoMapper Profile
/// Entity ve DTO arasındaki mapping (dönüşüm) kurallarını tanımlar
/// </summary>
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // CATEGORY MAPPINGS
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();

        // PRODUCT MAPPINGS
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.CompanyName));

        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        // CUSTOMER MAPPINGS
        CreateMap<Customer, CustomerDto>();
        CreateMap<RegisterCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>();

        // ORDER MAPPINGS
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"));

        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.OrderNumber, opt => opt.Ignore()) // Sipariş numarası service'te oluşturulacak
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.Pending))
            .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => false));

        // ORDER DETAIL MAPPINGS
        CreateMap<OrderDetail, OrderDetailDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));

        CreateMap<CreateOrderDetailDto, OrderDetail>()
            .ForMember(dest => dest.UnitPrice, opt => opt.Ignore()) // Service'te ürün fiyatından alınacak
            .ForMember(dest => dest.LineTotal, opt => opt.Ignore()); // Service'te hesaplanacak
    }
}

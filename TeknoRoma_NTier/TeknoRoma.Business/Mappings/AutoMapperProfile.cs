using AutoMapper;
using TeknoRoma.Business.DTOs;
using TeknoRoma.Entities;
using TeknoRoma.Entities.Enums;

namespace TeknoRoma.Business.Mappings;

/// <summary>
/// AutoMapper Profile - Entity ↔ DTO Mapping Kuralları
///
/// NEDEN AutoMapper?
/// - Manual mapping yerine otomatik dönüşüm
/// - Boilerplate kod azaltır (her entity için 50+ satır tasarruf)
/// - Complex mapping'leri merkezi yönetir
/// - Runtime'da performanslı çalışır (compile-time configuration)
///
/// MAPPING TİPLERİ:
/// - Entity → Dto: Veritabanından okuma (GET)
/// - CreateDto → Entity: Yeni kayıt oluşturma (POST)
/// - UpdateDto → Entity: Kayıt güncelleme (PUT)
///
/// DİKKAT!
/// - Navigation property mapping'leri dikkatli yapılmalı
/// - Circular reference'lara dikkat (Entity.Related.Entity...)
/// - Ignore edilen alanlar service layer'da set edilmeli
/// </summary>
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ====== CATEGORY MAPPINGS ======
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();


        // ====== PRODUCT MAPPINGS ======
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : ""))
            .ForMember(dest => dest.SupplierName,
                opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.CompanyName : ""));

        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();


        // ====== SUPPLIER MAPPINGS ======
        CreateMap<Supplier, SupplierDto>();
        CreateMap<CreateSupplierDto, Supplier>();
        CreateMap<UpdateSupplierDto, Supplier>();


        // ====== CUSTOMER MAPPINGS ======
        CreateMap<Customer, CustomerDto>();
        CreateMap<RegisterCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>();


        // ====== STORE MAPPINGS ======
        // Store → StoreDto: Read işlemleri için
        CreateMap<Store, StoreDto>()
            .ForMember(dest => dest.CurrentEmployeeCount, opt => opt.Ignore()); // Service'te hesaplanır

        // CreateStoreDto → Store: POST işlemleri için
        CreateMap<CreateStoreDto, Store>();

        // NOT: UpdateStoreDto'dan Store'a mapping yok
        // Çünkü Update işleminde sadece belirli alanlar güncellenir
        // Bu kontrol service layer'da yapılır


        // ====== DEPARTMENT MAPPINGS ======
        CreateMap<Department, DepartmentDto>()
            .ForMember(dest => dest.StoreName, opt => opt.Ignore())         // Service'te set edilir
            .ForMember(dest => dest.ManagerFullName, opt => opt.Ignore())   // Service'te set edilir
            .ForMember(dest => dest.EmployeeCount, opt => opt.Ignore());    // Service'te hesaplanır

        CreateMap<CreateDepartmentDto, Department>();


        // ====== EMPLOYEE MAPPINGS ======
        // Employee → EmployeeDto: Genel bilgi (Maaş YOK)
        CreateMap<Employee, EmployeeDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => DateTime.Now.Year - src.BirthDate.Year))
            .ForMember(dest => dest.YearsOfService, opt => opt.MapFrom(src => DateTime.Now.Year - src.HireDate.Year))
            .ForMember(dest => dest.StoreName, opt => opt.Ignore())         // Service'te set edilir
            .ForMember(dest => dest.DepartmentName, opt => opt.Ignore());   // Service'te set edilir

        // Employee → EmployeeDetailDto: Maaş bilgisi VAR (yetkili kişiler için)
        CreateMap<Employee, EmployeeDetailDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => DateTime.Now.Year - src.BirthDate.Year))
            .ForMember(dest => dest.YearsOfService, opt => opt.MapFrom(src => DateTime.Now.Year - src.HireDate.Year))
            .ForMember(dest => dest.StoreName, opt => opt.Ignore())
            .ForMember(dest => dest.DepartmentName, opt => opt.Ignore());

        // Employee → EmployeeSummaryDto: Liste görünümü için hafif DTO
        CreateMap<Employee, EmployeeSummaryDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.StoreName, opt => opt.Ignore())
            .ForMember(dest => dest.DepartmentName, opt => opt.Ignore());

        CreateMap<CreateEmployeeDto, Employee>()
            .ForMember(dest => dest.IdentityUserId, opt => opt.Ignore()); // ASP.NET Identity'den gelecek


        // ====== EXPENSE MAPPINGS ======
        CreateMap<Expense, ExpenseDto>()
            .ForMember(dest => dest.StoreName, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeFullName, opt => opt.Ignore());

        CreateMap<Expense, ExpenseSummaryDto>()
            .ForMember(dest => dest.StoreName, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeFullName, opt => opt.Ignore());

        // NOT: CreateExpenseDto → Expense mapping yok
        // Çünkü ExpenseNumber ve AmountInTRY service'te hesaplanır


        // ====== TECHNICAL SERVICE MAPPINGS ======
        CreateMap<TechnicalService, TechnicalServiceDto>()
            .ForMember(dest => dest.StoreName, opt => opt.Ignore())
            .ForMember(dest => dest.ReportedByEmployeeName, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedToEmployeeName, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
            .ForMember(dest => dest.ProductName, opt => opt.Ignore())
            .ForMember(dest => dest.ResolutionTimeInHours, opt => opt.MapFrom(src =>
                src.ResolvedDate.HasValue
                    ? (src.ResolvedDate.Value - src.ReportedDate).TotalHours
                    : (double?)null));

        CreateMap<TechnicalService, TechnicalServiceSummaryDto>()
            .ForMember(dest => dest.PriorityText, opt => opt.MapFrom(src =>
                src.Priority == 1 ? "Düşük" :
                src.Priority == 2 ? "Orta" :
                src.Priority == 3 ? "Yüksek" :
                src.Priority == 4 ? "Kritik" : "Belirsiz"))
            .ForMember(dest => dest.SlaStatus, opt => opt.Ignore())  // Service'te hesaplanır
            .ForMember(dest => dest.StoreName, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedToEmployeeName, opt => opt.Ignore());


        // ====== SALE MAPPINGS ======
        // Sale (Order yerine) - TeknoRoma terminolojisi
        CreateMap<Sale, SaleDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
            .ForMember(dest => dest.StoreName, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeName, opt => opt.Ignore())
            .ForMember(dest => dest.SaleDetails, opt => opt.Ignore()); // Ayrı sorgulanır

        CreateMap<Sale, SaleSummaryDto>()
            .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
            .ForMember(dest => dest.StoreName, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeName, opt => opt.Ignore())
            .ForMember(dest => dest.TotalItemCount, opt => opt.Ignore()); // Service'te hesaplanır


        // ====== SALE DETAIL MAPPINGS ======
        CreateMap<SaleDetail, SaleDetailDto>()
            .ForMember(dest => dest.ProductName, opt => opt.Ignore()); // Service'te set edilir

        // NOT: CreateSaleDto → Sale mapping yok
        // Çünkü SaleNumber, TotalAmount ve stok güncellemeleri service'te yapılır
    }
}

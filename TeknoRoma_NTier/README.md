# TeknoRoma E-Ticaret Platformu - N-tier Architecture

TeknoRoma, modern yazılım mimarisi prensiplerine uygun olarak **N-tier (Katmanlı) Mimari** ile geliştirilmiş kapsamlı bir e-ticaret platformudur.

## 📋 İçindekiler
- [Proje Hakkında](#proje-hakkında)
- [Mimari Yapı](#mimari-yapı)
- [Teknolojiler](#teknolojiler)
- [Katmanlar ve Sorumlulukları](#katmanlar-ve-sorumlulukları)
- [Design Patterns](#design-patterns)
- [Veritabanı Yapısı](#veritabanı-yapısı)
- [Kurulum](#kurulum)
- [Kullanım](#kullanım)
- [API Endpoints](#api-endpoints)

## 🎯 Proje Hakkında

TeknoRoma, teknoloji ürünlerinin satışını gerçekleştiren bir e-ticaret platformudur. Proje, **Separation of Concerns (SoC)** prensibi ile katmanlara ayrılmış, test edilebilir ve bakımı kolay bir yapıya sahiptir.

### Özellikler
- ✅ N-tier Katmanlı Mimari
- ✅ Repository Pattern & Unit of Work
- ✅ Dependency Injection
- ✅ AutoMapper ile DTO kullanımı
- ✅ Soft Delete implementasyonu
- ✅ RESTful API
- ✅ MVC Web Uygulaması
- ✅ Entity Framework Core
- ✅ Swagger API Dokümantasyonu

## 🏗️ Mimari Yapı

Proje 4 ana katmandan oluşur:

```
TeknoRoma_NTier/
│
├── 1-Entities (TeknoRoma.Entities)          # Varlık Katmanı
│   ├── BaseEntity.cs                        # Temel entity sınıfı
│   ├── Category.cs                          # Kategori entity
│   ├── Product.cs                           # Ürün entity
│   ├── Supplier.cs                          # Tedarikçi entity
│   ├── Customer.cs                          # Müşteri entity
│   ├── Order.cs                             # Sipariş entity
│   └── OrderDetail.cs                       # Sipariş detayı entity
│
├── 2-DataAccess (TeknoRoma.DataAccess)      # Veri Erişim Katmanı
│   ├── Context/
│   │   └── TeknoRomaDbContext.cs            # EF Core DbContext
│   ├── Abstract/
│   │   ├── IRepository.cs                   # Generic repository interface
│   │   └── IUnitOfWork.cs                   # Unit of Work interface
│   └── Concrete/
│       ├── Repository.cs                    # Generic repository implementation
│       └── UnitOfWork.cs                    # Unit of Work implementation
│
├── 3-Business (TeknoRoma.Business)          # İş Mantığı Katmanı
│   ├── DTOs/                                # Data Transfer Objects
│   │   ├── CategoryDto.cs
│   │   ├── ProductDto.cs
│   │   ├── CustomerDto.cs
│   │   └── OrderDto.cs
│   ├── Mappings/
│   │   └── AutoMapperProfile.cs             # AutoMapper konfigürasyonu
│   ├── Abstract/
│   │   ├── IProductService.cs               # Ürün service interface
│   │   └── ICategoryService.cs              # Kategori service interface
│   └── Concrete/
│       ├── ProductService.cs                # Ürün service implementation
│       └── CategoryService.cs               # Kategori service implementation
│
└── 4-Presentation                           # Sunum Katmanı
    ├── TeknoRoma.WebAPI/                    # RESTful API
    │   ├── Controllers/
    │   │   ├── ProductsController.cs        # Ürün API endpoints
    │   │   └── CategoriesController.cs      # Kategori API endpoints
    │   ├── Program.cs                       # API başlangıç noktası
    │   └── appsettings.json                 # API konfigürasyonu
    │
    └── TeknoRoma.WebMVC/                    # MVC Web Uygulaması
        ├── Controllers/
        │   ├── HomeController.cs            # Ana sayfa controller
        │   └── ProductsController.cs        # Ürün sayfaları controller
        ├── Views/
        │   ├── Home/Index.cshtml            # Ana sayfa view
        │   └── Shared/_Layout.cshtml        # Layout view
        ├── Program.cs                       # MVC başlangıç noktası
        └── appsettings.json                 # MVC konfigürasyonu
```

## 🛠️ Teknolojiler

### Backend
- **.NET 8.0** - Modern, cross-platform framework
- **ASP.NET Core Web API** - RESTful servisler için
- **ASP.NET Core MVC** - Web UI için
- **Entity Framework Core 8.0** - ORM (Object-Relational Mapping)
- **SQL Server** - İlişkisel veritabanı

### Kütüphaneler
- **AutoMapper 12.0** - Entity/DTO dönüşümleri
- **Swashbuckle (Swagger) 6.5** - API dokümantasyonu
- **Bootstrap 5.3** - Responsive UI framework
- **Font Awesome 6.4** - Icon kütüphanesi

## 📚 Katmanlar ve Sorumlulukları

### 1️⃣ Entities Layer (Varlık Katmanı)
**Sorumluluk:** Veritabanı tablolarına karşılık gelen model sınıflarını içerir.

**Neden Kullanılır:**
- Veritabanı şemasını kod olarak tanımlar
- Entity'ler arasındaki ilişkileri (relationships) belirtir
- Hiçbir katmana bağımlı değildir (Pure POCO)

**Önemli Noktalar:**
- `BaseEntity`: Tüm entity'lerin ortak özelliklerini içerir (Id, CreatedDate, UpdatedDate, IsDeleted)
- Virtual properties: Lazy loading için navigation properties
- Enums: OrderStatus, PaymentMethod gibi sabit değerler

### 2️⃣ DataAccess Layer (Veri Erişim Katmanı)
**Sorumluluk:** Veritabanı işlemlerini (CRUD) yönetir.

**Neden Kullanılır:**
- Database logic'i diğer katmanlardan izole eder
- Repository Pattern ile test edilebilirlik sağlar
- Unit of Work ile transaction yönetimi yapar

**Önemli Noktalar:**
- **DbContext**: EF Core'un veritabanı ile iletişim kurmasını sağlar
  - Fluent API ile ilişkileri yapılandırır
  - Global Query Filter ile Soft Delete implementasyonu
  - SaveChanges override ile otomatik timestamp güncelleme

- **Repository Pattern**: Generic CRUD işlemleri
  - `GetByIdAsync()`: ID ile kayıt getirme
  - `GetAllAsync()`: Tüm kayıtları getirme
  - `FindAsync()`: Lambda expression ile filtreleme
  - `AddAsync()`, `Update()`, `Delete()`: Veri işlemleri
  - `SoftDelete()`: Mantıksal silme (IsDeleted = true)

- **Unit of Work Pattern**: Transaction yönetimi
  - Birden fazla repository işlemini tek transaction'da yönetir
  - `SaveChangesAsync()`: Tüm değişiklikleri commit eder
  - `BeginTransactionAsync()`, `CommitTransactionAsync()`, `RollbackTransactionAsync()`

### 3️⃣ Business Layer (İş Mantığı Katmanı)
**Sorumluluk:** İş kurallarını, validasyonları ve business logic'i içerir.

**Neden Kullanılır:**
- İş kurallarını merkezi bir yerde toplar
- Controller'ları şişirmeden iş mantığını yönetir
- DTO'lar ile güvenli veri transferi sağlar

**Önemli Noktalar:**
- **DTOs (Data Transfer Objects)**:
  - Entity'leri dış dünyaya expose etmez (güvenlik)
  - Sadece gerekli alanları içerir
  - CreateDto, UpdateDto, ReadDto ayrımı
  - Örnek: Password alanı DTO'da yer almaz

- **AutoMapper**: Entity ↔ DTO dönüşümleri
  - Manuel mapping yerine konfigürasyon tabanlı
  - `CreateMap<Source, Destination>()`
  - ForMember ile custom mapping'ler

- **Services**: Business logic implementation
  - Validation: İş kuralı kontrolleri
  - Örnek: İndirimli fiyat, normal fiyattan düşük olmalı
  - Örnek: Kategoriye ait ürün varsa kategori silinemez
  - Repository'leri kullanarak CRUD işlemlerini gerçekleştirir

### 4️⃣ Presentation Layer (Sunum Katmanı)
**Sorumluluk:** Kullanıcı ile etkileşimi sağlar (UI/API).

**Neden Kullanılır:**
- Web API: Frontend uygulamaları için RESTful servisler
- Web MVC: Server-side rendered web sayfaları

**Web.API Özellikleri:**
- RESTful endpoint'ler (GET, POST, PUT, DELETE)
- HTTP status code'ları (200 OK, 404 Not Found, 500 Internal Server Error)
- Swagger UI: API dokümantasyonu ve test arayüzü
- CORS: Frontend uygulamalarının API'yi çağırabilmesi için

**Web.MVC Özellikleri:**
- Controller: İstekleri karşılar, service'leri çağırır
- View: Razor syntax ile dinamik HTML oluşturur
- Model Binding: Form verilerini otomatik DTO'ya dönüştürür
- TempData: Sayfalar arası mesaj taşıma
- ViewBag: Controller'dan View'a veri gönderme

## 🎨 Design Patterns

### 1. Repository Pattern
**Amaç:** Veri erişim katmanını soyutlar ve test edilebilirliği artırır.

**Nasıl Çalışır:**
```csharp
// Interface tanımı
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    // ...
}

// Kullanım
var product = await _unitOfWork.Products.GetByIdAsync(5);
```

**Avantajları:**
- Veritabanı implementasyonu değişse bile interface aynı kalır
- Mock'lanabilir (unit test için)
- Tek bir yerde değişiklik yaparak tüm entity'lere etki edilir

### 2. Unit of Work Pattern
**Amaç:** Birden fazla repository işlemini tek bir transaction içinde yönetir.

**Nasıl Çalışır:**
```csharp
// Sipariş + Sipariş Detayları ekleme (tek transaction)
await _unitOfWork.BeginTransactionAsync();
try
{
    await _unitOfWork.Orders.AddAsync(order);
    await _unitOfWork.OrderDetails.AddRangeAsync(orderDetails);
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
}
```

**Avantajları:**
- ACID prensiplerini sağlar
- Hata durumunda tüm işlemleri geri alır
- Transaction koordinasyonu

### 3. Dependency Injection (DI)
**Amaç:** Sınıflar arası bağımlılıkları azaltır, loose coupling sağlar.

**Nasıl Çalışır:**
```csharp
// Program.cs - Service Registration
builder.Services.AddScoped<IProductService, ProductService>();

// Controller - Constructor Injection
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
}
```

**Avantajları:**
- Testable: Mock service'ler enjekte edilebilir
- Maintainable: Interface değişmediği sürece implementation değiştirilebilir
- Lifecycle Management: Scoped, Transient, Singleton

## 💾 Veritabanı Yapısı

### Entity'ler ve İlişkiler

#### Category (Kategori)
- **İlişki**: One-to-Many → Product
- **Alanlar**: Name, Description, ImageUrl, IsActive

#### Product (Ürün)
- **İlişki**:
  - Many-to-One → Category
  - Many-to-One → Supplier
  - One-to-Many → OrderDetail
- **Alanlar**: Name, Description, Price, DiscountPrice, Stock, ImageUrl, IsActive, IsFeatured

#### Supplier (Tedarikçi)
- **İlişki**: One-to-Many → Product
- **Alanlar**: CompanyName, ContactName, Phone, Email, Address

#### Customer (Müşteri)
- **İlişki**: One-to-Many → Order
- **Alanlar**: FirstName, LastName, Email, Password, Phone, Address, IsEmailVerified

#### Order (Sipariş)
- **İlişki**:
  - Many-to-One → Customer
  - One-to-Many → OrderDetail
- **Alanlar**: OrderNumber, OrderDate, Status, PaymentMethod, ShippingAddress, TotalAmount

#### OrderDetail (Sipariş Detayı)
- **İlişki**:
  - Many-to-One → Order
  - Many-to-One → Product
- **Alanlar**: Quantity, UnitPrice, DiscountRate, LineTotal

### Soft Delete Mekanizması
Tüm entity'lerde `IsDeleted` flag'i bulunur. Silme işlemlerinde:
- **Soft Delete**: `IsDeleted = true` yapılır, fiziksel olarak silinmez
- **Global Query Filter**: IsDeleted = false olanlar otomatik getirilir
- **Avantajı**: Veri kaybı olmaz, gerektiğinde geri getirilebilir

## 🚀 Kurulum

### Gereksinimler
- .NET 8.0 SDK
- SQL Server (LocalDB veya Express)
- Visual Studio 2022 veya VS Code

### Adımlar

1. **Repository'yi Klonlayın**
```bash
git clone <repository-url>
cd TeknoRoma_NTier
```

2. **Connection String'i Güncelleyin**

`TeknoRoma.WebAPI/appsettings.json` ve `TeknoRoma.WebMVC/appsettings.json` dosyalarındaki connection string'i kendi SQL Server bağlantınıza göre düzenleyin:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TeknoRomaDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

3. **Migration ve Database Oluşturma**

```bash
cd TeknoRoma.DataAccess
dotnet ef migrations add InitialCreate --startup-project ../TeknoRoma.WebAPI/
dotnet ef database update --startup-project ../TeknoRoma.WebAPI/
```

4. **Projeleri Çalıştırın**

**Web API:**
```bash
cd TeknoRoma.WebAPI
dotnet run
```
Swagger UI: https://localhost:5001

**Web MVC:**
```bash
cd TeknoRoma.WebMVC
dotnet run
```
Web UI: https://localhost:5002

## 📖 Kullanım

### Web API Kullanımı

Swagger UI üzerinden API'yi test edebilirsiniz: `https://localhost:5001`

**Örnek İstekler:**

```bash
# Tüm ürünleri getir
GET https://localhost:5001/api/Products

# ID'ye göre ürün getir
GET https://localhost:5001/api/Products/5

# Yeni ürün ekle
POST https://localhost:5001/api/Products
Content-Type: application/json

{
  "name": "iPhone 15 Pro",
  "description": "Apple'ın en yeni modeli",
  "price": 45000,
  "stock": 50,
  "categoryId": 1,
  "supplierId": 1
}
```

### Web MVC Kullanımı

Browser'da `https://localhost:5002` adresini açın:
- Ana Sayfa: Öne çıkan ürünler ve kategoriler
- Ürünler: Tüm ürün listesi, filtreleme ve arama
- Ürün Detay: Detaylı ürün bilgileri

## 🔗 API Endpoints

### Products

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/Products` | Tüm ürünleri listele |
| GET | `/api/Products/{id}` | ID'ye göre ürün getir |
| GET | `/api/Products/category/{categoryId}` | Kategoriye göre ürünler |
| GET | `/api/Products/featured` | Öne çıkan ürünler |
| GET | `/api/Products/search?term={searchTerm}` | Ürün arama |
| POST | `/api/Products` | Yeni ürün ekle |
| PUT | `/api/Products/{id}` | Ürün güncelle |
| DELETE | `/api/Products/{id}` | Ürün sil (Soft Delete) |
| PATCH | `/api/Products/{id}/stock?quantity={quantity}` | Stok güncelle |

### Categories

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/Categories` | Tüm kategorileri listele |
| GET | `/api/Categories/active` | Aktif kategorileri listele |
| GET | `/api/Categories/{id}` | ID'ye göre kategori getir |
| POST | `/api/Categories` | Yeni kategori ekle |
| PUT | `/api/Categories/{id}` | Kategori güncelle |
| DELETE | `/api/Categories/{id}` | Kategori sil (Soft Delete) |

## 🎓 N-tier Mimarinin Avantajları

### 1. Separation of Concerns (SoC)
Her katman kendi sorumluluğundaki işleri yapar:
- Entities: Sadece veri modeli
- DataAccess: Sadece veritabanı işlemleri
- Business: Sadece iş kuralları
- Presentation: Sadece kullanıcı arayüzü

### 2. Maintainability (Bakım Kolaylığı)
- Bir katmandaki değişiklik diğer katmanları etkilemez
- Örnek: Veritabanı SQL Server'dan PostgreSQL'e değişirse, sadece DataAccess katmanı güncellenir

### 3. Testability (Test Edilebilirlik)
- Her katman bağımsız test edilebilir
- Mock/Stub objeler kolayca kullanılabilir
- Unit test, Integration test yazılabilir

### 4. Reusability (Yeniden Kullanılabilirlik)
- Business katmanı hem API hem MVC tarafından kullanılır
- Repository'ler tüm entity'ler için generic olarak çalışır

### 5. Scalability (Ölçeklenebilirlik)
- Katmanlar farklı sunucularda çalıştırılabilir
- Load balancing yapılabilir
- Microservice'lere dönüşüm kolay

### 6. Security (Güvenlik)
- Entity'ler direkt dışarıya expose edilmez
- DTO'lar ile sadece gerekli veriler paylaşılır
- Katmanlar arası güvenlik kontrolleri

## 📝 Öğrenme Notları

### N-tier vs Onion Architecture Karşılaştırması

| Özellik | N-tier | Onion Architecture |
|---------|--------|-------------------|
| **Bağımlılık Yönü** | Yukarıdan aşağıya (Presentation → Business → DataAccess → Entities) | Dışarıdan içeriye (tüm katmanlar Core'a bağımlı) |
| **Core Katmanı** | Entities en alt katmandır | Domain en merkezde ve bağımsızdır |
| **DataAccess** | Business katmanına bağımlıdır | Infrastructure katmanındadır, Domain'e bağımlıdır |
| **Kullanım Senaryosu** | Klasik enterprise uygulamalar | DDD (Domain-Driven Design) uygulamalar |
| **Complexity** | Daha basit ve anlaşılır | Daha kompleks ama esnek |

### Hangi Mimariyi Seçmeliyim?

**N-tier Architecture için:**
- CRUD ağırlıklı uygulamalar
- Klasik e-ticaret, CMS, Admin panelleri
- Takım arkadaşları N-tier'e aşina

**Onion Architecture için:**
- Karmaşık iş kuralları
- Domain-Driven Design gerekli
- Microservice mimarisi
- Yüksek test coverage gerekli

## 🔍 Kod Örnekleri ve Açıklamalar

### Entity Framework Core - Fluent API
```csharp
// TeknoRomaDbContext.cs
modelBuilder.Entity<Product>(entity =>
{
    // Primary Key tanımı
    entity.HasKey(e => e.Id);

    // Property konfigürasyonları
    entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();

    // İlişki tanımı (Foreign Key)
    entity.HasOne(p => p.Category)
          .WithMany(c => c.Products)
          .HasForeignKey(p => p.CategoryId)
          .OnDelete(DeleteBehavior.Restrict);

    // Index tanımı (performans için)
    entity.HasIndex(e => e.Name);
});
```

**Açıklama:**
- `HasKey`: Primary key belirtir
- `IsRequired`: NOT NULL constraint
- `HasMaxLength`: VARCHAR(200)
- `HasColumnType`: SQL veri tipi
- `HasOne/WithMany`: İlişki tanımı
- `OnDelete(Restrict)`: Cascade delete'i engeller

### AutoMapper Mapping
```csharp
// AutoMapperProfile.cs
CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.CategoryName,
               opt => opt.MapFrom(src => src.Category.Name))
    .ForMember(dest => dest.SupplierName,
               opt => opt.MapFrom(src => src.Supplier.CompanyName));
```

**Açıklama:**
- `CreateMap`: Source → Destination mapping
- `ForMember`: Custom mapping kuralı
- `MapFrom`: Kaynak property'den mapping

### Dependency Injection Lifecycle

```csharp
// Program.cs
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();        // HTTP request başına 1 instance
builder.Services.AddTransient<IEmailService, EmailService>(); // Her inject'te yeni instance
builder.Services.AddSingleton<ICacheService, CacheService>(); // Uygulama boyunca 1 instance
```

**Açıklama:**
- **Scoped**: HTTP request süresince aynı instance
- **Transient**: Her seferinde yeni instance (stateless service'ler için)
- **Singleton**: Uygulama başladığında 1 kez oluşturulur (cache, configuration)

## 🎉 Sonuç

Bu proje, **N-tier Katmanlı Mimari**'nin tüm özelliklerini gösterir:
- ✅ Katmanlı yapı ile separation of concerns
- ✅ Repository Pattern ve Unit of Work
- ✅ Dependency Injection
- ✅ DTO'lar ile güvenli veri transferi
- ✅ RESTful API ve MVC kullanımı
- ✅ Entity Framework Core ile ORM
- ✅ Soft Delete implementasyonu
- ✅ Business logic ve validasyonlar

Proje, hem öğrenme hem de gerçek dünya uygulamaları için sağlam bir temel sunmaktadır.

---

**Geliştirici:** TeknoRoma Development Team
**Tarih:** 2024
**Lisans:** MIT

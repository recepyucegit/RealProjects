# Infrastructure Katmanı

## Genel Bakış

Infrastructure katmanı, **Onion Architecture**'da en dış katmandır. Tüm dış bağımlılıklar (veritabanı, API'ler, dosya sistemi) bu katmanda yer alır.

```
┌─────────────────────────────────────────────────────────┐
│                    INFRASTRUCTURE                        │  ← En dış katman (Bu katman)
│  ┌─────────────────────────────────────────────────┐    │
│  │                  APPLICATION                     │    │
│  │  ┌─────────────────────────────────────────┐    │    │
│  │  │                 DOMAIN                   │    │    │  ← En iç katman
│  │  │           (Entities, Enums)              │    │    │
│  │  └─────────────────────────────────────────┘    │    │
│  │        (Interfaces, DTOs, Services)             │    │
│  └─────────────────────────────────────────────────┘    │
│      (DbContext, Repositories, External Services)       │
└─────────────────────────────────────────────────────────┘
```

## Klasör Yapısı

```
Infrastructure/
├── DependencyInjection/
│   └── InfrastructureServiceRegistration.cs   # DI kayıtları
├── Persistence/
│   ├── AppDbContext.cs                        # EF Core DbContext
│   ├── Configurations/                        # Entity konfigürasyonları
│   │   ├── ProductConfiguration.cs
│   │   ├── CategoryConfiguration.cs
│   │   ├── SupplierConfiguration.cs
│   │   ├── CustomerConfiguration.cs
│   │   ├── EmployeeConfiguration.cs
│   │   ├── SaleConfiguration.cs
│   │   ├── SaleDetailConfiguration.cs
│   │   ├── StoreConfiguration.cs
│   │   ├── DepartmentConfiguration.cs
│   │   ├── ExpenseConfiguration.cs
│   │   ├── TechnicalServiceConfiguration.cs
│   │   └── SupplierTransactionConfiguration.cs
│   └── Repositories/                          # Repository implementasyonları
│       ├── EfRepository.cs                    # Generic repository
│       ├── UnitOfWork.cs                      # Unit of Work pattern
│       ├── ProductRepository.cs
│       ├── CustomerRepository.cs
│       ├── EmployeeRepository.cs
│       ├── SaleRepository.cs
│       ├── SaleDetailRepository.cs
│       ├── ExpenseRepository.cs
│       ├── TechnicalServiceRepository.cs
│       └── SupplierTransactionRepository.cs
├── Services/
│   └── ExchangeRateService.cs                 # TCMB döviz kuru servisi
├── Infrastructure.csproj                       # Proje dosyası
└── README.md                                   # Bu dosya
```

## Bağımlılıklar (NuGet Paketleri)

| Paket | Versiyon | Açıklama |
|-------|----------|----------|
| Microsoft.EntityFrameworkCore | 8.0.0 | EF Core temel paket |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.0 | SQL Server provider |
| Microsoft.EntityFrameworkCore.Tools | 8.0.0 | Migration komutları |
| Microsoft.EntityFrameworkCore.Design | 8.0.0 | Design-time servisleri |

## Kurulum ve Yapılandırma

### 1. Connection String (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-E0P9L99\\SQLEXPRESS01;Database=TeknoRoma;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 2. Program.cs Yapılandırması

```csharp
using Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure servislerini kaydet
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();
```

### 3. Migration Komutları

```bash
# Package Manager Console (Visual Studio)
Add-Migration InitialCreate -Project Infrastructure -StartupProject WebAPI
Update-Database -Project Infrastructure -StartupProject WebAPI

# .NET CLI
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project WebAPI
dotnet ef database update --project Infrastructure --startup-project WebAPI
```

## Bileşenler

### 1. AppDbContext

Entity Framework Core DbContext sınıfı. Veritabanı bağlantısı ve entity yönetimi.

```csharp
public class AppDbContext : DbContext
{
    // DbSet tanımları
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    // ... diğer entity'ler

    // Otomatik audit alanları (SaveChanges override)
    // - CreatedDate: Kayıt oluşturulduğunda
    // - ModifiedDate: Kayıt güncellendiğinde
}
```

**Özellikler:**
- Soft Delete için Global Query Filter
- Otomatik CreatedDate/ModifiedDate
- ApplyConfigurationsFromAssembly ile otomatik configuration yükleme

### 2. Entity Configurations

Fluent API ile entity yapılandırmaları. Her entity için ayrı configuration dosyası.

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        // ... diğer konfigürasyonlar
    }
}
```

**Avantajları:**
- DbContext temiz kalır
- Her entity kendi dosyasında yönetilir
- Separation of Concerns prensibi

### 3. Repository Pattern

#### Generic Repository (EfRepository<T>)

Tüm entity'ler için ortak CRUD operasyonları.

```csharp
public class EfRepository<T> : IRepository<T> where T : BaseEntity
{
    // Temel CRUD metodları
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);        // Hard delete
    Task SoftDeleteAsync(T entity);    // Soft delete
}
```

#### Entity-Specific Repositories

Her entity için özel sorgular içeren repository'ler.

| Repository | Özel Metodlar |
|------------|---------------|
| ProductRepository | GetByBarcodeAsync, GetLowStockProductsAsync, UpdateStockAsync |
| CustomerRepository | GetByIdentityNumberAsync, GetTopCustomersAsync |
| EmployeeRepository | GetByIdentityUserIdAsync, GetTopSellersAsync |
| SaleRepository | GetBySaleNumberAsync, GetDailyTotalAsync, GenerateSaleNumberAsync |
| SaleDetailRepository | GetBySaleAsync, GetTopSellingProductsAsync |
| ExpenseRepository | GetUnpaidExpensesAsync, GetMonthlyTotalAsync |
| TechnicalServiceRepository | GetOpenIssuesAsync, GetUnassignedAsync |
| SupplierTransactionRepository | GetUnpaidTransactionsAsync, GetBySupplierAsync |

### 4. Unit of Work Pattern

Tüm repository'lere tek noktadan erişim ve transaction yönetimi.

```csharp
public interface IUnitOfWork : IDisposable
{
    // Repository erişimi
    IProductRepository Products { get; }
    ICustomerRepository Customers { get; }
    // ... diğer repository'ler

    // Transaction yönetimi
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<int> SaveChangesAsync();
}
```

**Kullanım Örneği:**

```csharp
public class SaleService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task CreateSaleAsync(CreateSaleDto dto)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 1. Satış oluştur
            var sale = new Sale { ... };
            await _unitOfWork.Sales.AddAsync(sale);

            // 2. Stok düş
            await _unitOfWork.Products.UpdateStockAsync(productId, -quantity);

            // 3. Transaction'ı onayla
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

### 5. External Services

#### ExchangeRateService

TCMB (Türkiye Cumhuriyet Merkez Bankası) API entegrasyonu.

```csharp
public interface IExchangeRateService
{
    Task<decimal> GetUsdRateAsync();                              // USD/TRY kuru
    Task<decimal> GetEurRateAsync();                              // EUR/TRY kuru
    Task<decimal> ConvertToTryAsync(decimal amount, string currency); // Döviz çevirme
}
```

**Özellikler:**
- TCMB XML API: `https://www.tcmb.gov.tr/kurlar/today.xml`
- MemoryCache ile 1 saat cache
- Fallback kurlar (API erişilemezse)

## Dependency Injection

### Servis Yaşam Süreleri (Lifetime)

| Lifetime | Açıklama | Kullanım |
|----------|----------|----------|
| **Transient** | Her istek için yeni instance | Hafif, stateless servisler |
| **Scoped** | Her HTTP request için bir instance | DbContext, Repository, UnitOfWork |
| **Singleton** | Uygulama boyunca tek instance | HttpClient, MemoryCache |

### Kayıt Edilen Servisler

```csharp
// InfrastructureServiceRegistration.cs
public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // DbContext (Scoped)
    services.AddDbContext<AppDbContext>(options => ...);

    // Generic Repository (Scoped)
    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

    // Entity-Specific Repositories (Scoped)
    services.AddScoped<IProductRepository, ProductRepository>();
    services.AddScoped<ICustomerRepository, CustomerRepository>();
    // ... diğerleri

    // Unit of Work (Scoped)
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    // External Services
    services.AddHttpClient();
    services.AddMemoryCache();
    services.AddScoped<IExchangeRateService, ExchangeRateService>();

    return services;
}
```

## Mimari Prensipler

### 1. Dependency Inversion Principle (DIP)

```
Application Layer:  IProductRepository (Interface)
        ↑
Infrastructure Layer: ProductRepository (Implementation)
```

- Üst katmanlar (Application) alt katmanlara (Infrastructure) bağımlı DEĞİL
- Her iki katman da soyutlamalara (interface) bağımlı
- Bu sayede Infrastructure değiştirilebilir (örn: SQL Server → PostgreSQL)

### 2. Repository Pattern

**Avantajları:**
- Veritabanı erişim mantığı merkezi konumda
- Unit test için mock'lanabilir
- Sorgu optimizasyonu tek noktadan yapılabilir
- DbContext doğrudan kullanılmaz

### 3. Unit of Work Pattern

**Avantajları:**
- Birden fazla repository işlemi tek transaction'da
- Atomik operasyonlar (ya hep ya hiç)
- DbContext yaşam döngüsü yönetimi

### 4. Soft Delete

```csharp
// Entity'de
public bool IsDeleted { get; set; }
public DateTime? DeletedDate { get; set; }

// Global Query Filter (DbContext)
builder.HasQueryFilter(e => !e.IsDeleted);

// Soft Delete metodu
public async Task SoftDeleteAsync(T entity)
{
    entity.IsDeleted = true;
    entity.DeletedDate = DateTime.Now;
    await UpdateAsync(entity);
}
```

## Test Edilebilirlik

### Mock Repository Örneği

```csharp
// Unit Test
[Fact]
public async Task GetProduct_ShouldReturnProduct_WhenExists()
{
    // Arrange
    var mockRepo = new Mock<IProductRepository>();
    mockRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product { Id = 1, Name = "iPhone 15" });

    var service = new ProductService(mockRepo.Object);

    // Act
    var result = await service.GetByIdAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("iPhone 15", result.Name);
}
```

## Genişletilebilirlik

### Yeni Repository Ekleme

1. Application katmanında interface oluştur:
```csharp
// Application/Repositories/INewEntityRepository.cs
public interface INewEntityRepository : IRepository<NewEntity>
{
    Task<NewEntity?> GetBySpecialFieldAsync(string field);
}
```

2. Infrastructure'da implementation yaz:
```csharp
// Infrastructure/Persistence/Repositories/NewEntityRepository.cs
public class NewEntityRepository : EfRepository<NewEntity>, INewEntityRepository
{
    public async Task<NewEntity?> GetBySpecialFieldAsync(string field)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.SpecialField == field);
    }
}
```

3. DI'a kaydet:
```csharp
services.AddScoped<INewEntityRepository, NewEntityRepository>();
```

### Yeni External Service Ekleme

1. Application'da interface:
```csharp
// Application/Services/IEmailService.cs
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}
```

2. Infrastructure'da implementation:
```csharp
// Infrastructure/Services/SmtpEmailService.cs
public class SmtpEmailService : IEmailService
{
    public async Task SendAsync(string to, string subject, string body)
    {
        // SMTP implementation
    }
}
```

## İlgili Dosyalar

| Katman | Dosya | Açıklama |
|--------|-------|----------|
| Domain | Entities/*.cs | Entity sınıfları |
| Domain | Enums/*.cs | Enum tanımları |
| Application | Repositories/I*.cs | Repository interface'leri |
| Application | Services/I*.cs | Service interface'leri |
| Application | DTOs/*.cs | Data Transfer Objects |

## Versiyon Geçmişi

| Tarih | Değişiklik |
|-------|------------|
| 2024 | Initial Infrastructure setup |
| 2024 | Entity Configurations eklendi |
| 2024 | Repository implementations eklendi |
| 2024 | UnitOfWork pattern eklendi |
| 2024 | ExchangeRateService eklendi |
| 2024 | Dependency Injection yapılandırması |

---

**TeknoRoma - Onion Architecture Educational Project**

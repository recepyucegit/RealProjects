# Application Layer

Application katmanı, Onion Architecture'ın ikinci katmanıdır. Domain katmanına bağımlıdır ve iş mantığı arayüzlerini, DTO'ları ve repository arayüzlerini içerir.

## Klasör Yapısı

```
Application/
├── DTOs/                    # Data Transfer Objects
├── Repositories/            # Repository Interfaces
├── Services/                # Service Interfaces
└── README.md
```

## Repositories (Repository Arayüzleri)

Repository pattern, veri erişim katmanını soyutlar. Infrastructure katmanında implement edilir.

### IRepository<T> (Generic Repository)

Tüm entity'ler için temel CRUD operasyonları:

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();
}
```

### ISimpleRepository<T>

Basit entity'ler (Category, Supplier, Store, Department) için:

```csharp
public interface ISimpleRepository<T> : IRepository<T> where T : BaseEntity
{
    Task<T?> GetByNameAsync(string name);
    Task<bool> IsNameTakenAsync(string name, int? excludeId = null);
}
```

### Özelleştirilmiş Repository'ler

| Repository | Açıklama |
|------------|----------|
| `IProductRepository` | Barkod ile arama, kategori/tedarikçi filtreleme, stok güncelleme |
| `ICustomerRepository` | Telefon/email ile arama, puan güncelleme |
| `IEmployeeRepository` | Email ile arama, mağaza/departman filtreleme, authentication |
| `ISaleRepository` | Tarih aralığı, müşteri/çalışan/mağaza filtreleme |
| `ISaleDetailRepository` | Satış detayları, ürün bazlı satış sorgulama |
| `IExpenseRepository` | Tarih ve tip bazlı filtreleme, toplam hesaplama |
| `ITechnicalServiceRepository` | Durum bazlı filtreleme, teknisyen ataması |
| `ISupplierTransactionRepository` | Ödeme durumu filtreleme, bakiye hesaplama |

### IUnitOfWork

Transaction yönetimi ve tüm repository'lere tek noktadan erişim:

```csharp
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ICustomerRepository Customers { get; }
    IEmployeeRepository Employees { get; }
    ISaleRepository Sales { get; }
    // ... diğer repository'ler

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

## Services (Servis Arayüzleri)

İş mantığı katmanı. Presentation katmanından çağrılır, Repository'leri kullanır.

### Entity Servisleri

| Servis | Temel Operasyonlar |
|--------|-------------------|
| `IProductService` | CRUD, barkod kontrolü, stok güncelleme, arama |
| `ICustomerService` | CRUD, puan yönetimi, müşteri arama |
| `IEmployeeService` | CRUD, authentication, şifre yönetimi |
| `ISaleService` | Satış oluşturma, iptal, detay görüntüleme |
| `IExpenseService` | Gider CRUD, raporlama |
| `ITechnicalServiceService` | Servis kaydı, durum güncelleme, teknisyen atama |
| `ISupplierTransactionService` | İşlem CRUD, ödeme, bakiye hesaplama |

### Basit Entity Servisleri (ISimpleServices.cs)

```csharp
ICategoryService      // Kategori yönetimi
ISupplierService      // Tedarikçi yönetimi
IStoreService         // Mağaza yönetimi
IDepartmentService    // Departman yönetimi
```

### Özel Servisler

#### IReportService
Dashboard ve raporlama servisi:
- Günlük/Aylık satış raporları
- En çok satan ürünler
- Çalışan performans raporları
- Stok raporları
- Kar/Zarar analizi

#### IExchangeRateService
TCMB döviz kuru entegrasyonu:
- Güncel kurları çekme (USD, EUR, GBP)
- Para birimi dönüştürme
- Dövizli işlem desteği

## DTOs (Data Transfer Objects)

Katmanlar arası veri transferi için kullanılır. Domain entity'lerini doğrudan expose etmez.

### DTO Kategorileri

#### Entity DTO'ları
Her entity için:
- `[Entity]Dto` - Görüntüleme için (computed property'ler dahil)
- `Create[Entity]Dto` - Oluşturma için
- `Update[Entity]Dto` - Güncelleme için

#### Common DTO'lar (CommonDTOs.cs)

| DTO | Açıklama |
|-----|----------|
| `PaginatedResult<T>` | Sayfalama sonucu wrapper |
| `ApiResponse<T>` | API yanıt wrapper (Success, Message, Data, Errors) |
| `OperationResult` | Basit işlem sonucu |
| `PaginationRequest` | Sayfalama parametreleri |
| `SearchRequest` | Arama/filtreleme parametreleri |
| `SelectItemDto` | Dropdown listeler için |
| `DashboardSummaryDto` | Dashboard özet verileri |

#### Report DTO'ları (ReportDTOs.cs)

| DTO | Açıklama |
|-----|----------|
| `SalesReportDto` | Satış raporu |
| `TopSellingProductReportDto` | En çok satan ürünler |
| `EmployeeSalesReportDto` | Çalışan satış performansı |
| `StockReportDto` | Stok durumu raporu |
| `ProfitLossReportDto` | Kar/Zarar raporu |
| `CustomerReportDto` | Müşteri analizi |
| `TechnicalServiceReportDto` | Teknik servis raporu |

#### Exchange Rate DTO'ları

| DTO | Açıklama |
|-----|----------|
| `ExchangeRateDto` | Döviz kuru bilgisi |
| `CurrencyConversionDto` | Dönüştürme sonucu |
| `ExchangeRatesResponseDto` | Tüm kurlar yanıtı |

## Kullanım Örneği

```csharp
// Controller'da kullanım
public class ProductController : Controller
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync();
        return View(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        if (await _productService.IsBarcodeTakenAsync(dto.Barcode))
        {
            ModelState.AddModelError("Barcode", "Bu barkod zaten kullanılıyor");
            return View(dto);
        }

        var product = await _productService.CreateAsync(dto);
        return RedirectToAction(nameof(Index));
    }
}
```

## Bağımlılıklar

```
Application
    └── Domain (Entity'ler ve Enum'lar)
```

## Notlar

- Bu katman sadece **interface** ve **DTO** içerir
- Implementation'lar **Infrastructure** katmanında yapılır
- **Nullable** reference types aktif (`= null!` kullanımı)
- Tüm async metodlar `Task` veya `Task<T>` döner
- Repository'ler `IDisposable` pattern kullanmaz (UnitOfWork yönetir)

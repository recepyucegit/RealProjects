# TEKNOROMA - APPLICATION LAYER

## 📋 GENEL BAKIŞ

Application Layer, **iş mantığı koordinasyonundan** sorumludur. Domain ve Infrastructure arasında köprü görevi görür.

### ✅ Application Layer Özellikleri:
- **Domain'e bağımlıdır** (Entities, Enums kullanır)
- **Infrastructure'a bağımlı DEĞİLDİR** (Sadece interface'ler tanımlar)
- **UI'a bağımlı DEĞİLDİR** (DTO'larla çalışır)

### 🎯 Sorumlulukları:
1. **Repository Interface'lerini tanımlar** (IProductRepository, ISaleRepository...)
2. **Service Interface'lerini tanımlar** (IProductService, ISaleService...)
3. **DTO'ları tanımlar** (ProductDTO, CreateProductDTO, UpdateProductDTO...)
4. **İş mantığı koordinasyonu** (Transaction, Validation, Mapping)

---

## 📁 KLASÖR YAPISI

```
Application/
├── Repositories/               # Repository Interface'leri
│   ├── IRepository.cs                  # Generic base repository
│   ├── IProductRepository.cs           # Ürün işlemleri
│   ├── ISaleRepository.cs              # Satış işlemleri
│   ├── ICustomerRepository.cs          # Müşteri işlemleri
│   ├── IEmployeeRepository.cs          # Çalışan işlemleri
│   ├── ISimpleRepositories.cs          # Basit repository'ler
│   └── ITransactionRepositories.cs     # Gider, Teknik Servis vb.
│
├── Services/                   # Service Interface'leri
│   ├── IProductService.cs              # Ürün iş mantığı
│   ├── ISaleService.cs                 # Satış iş mantığı
│   ├── IBasicServices.cs               # Temel servisler
│   └── IComplexServices.cs             # Karmaşık iş mantığı
│
└── DTOs/                       # Data Transfer Objects
    ├── ProductDTOs/
    │   └── ProductDTOs.cs              # ProductDTO, CreateProductDTO, UpdateProductDTO
    ├── SaleDTOs/
    │   └── SaleDTOs.cs                 # SaleDTO, CreateSaleDTO...
    ├── CustomerDTOs/
    │   └── CustomerDTOs.cs
    ├── EmployeeDTOs/
    ├── CategoryDTOs/
    ├── SupplierDTOs/
    ├── ExpenseDTOs/
    ├── TechnicalServiceDTOs/
    ├── CommonDTOs.cs                   # Ortak DTO'lar
    └── TransactionDTOs.cs              # İşlem DTO'ları
```

---

## 🔍 REPOSITORY PATTERN

### NEDEN Repository Pattern?

```
❌ KÖTÜ YÖNTEM (Controller'da direkt DbContext):
Controller → DbContext (Sıkı bağımlılık, test edilemez)

✅ İYİ YÖNTEM (Repository Pattern):
Controller → Service → Repository Interface → Repository Implementation → DbContext
```

**Avantajları:**
1. **Loose Coupling** - Gevşek bağımlılık
2. **Testability** - Mock repository ile test edilebilir
3. **Değiştirilebilirlik** - Database değişse sadece implementasyon değişir
4. **Kod tekrarını önler** - Generic repository ile CRUD operasyonları tek yerde

### Generic Repository Pattern

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
```

**NEDEN Generic?**
- Her entity için ayrı interface yazmaktan kurtarır
- Product, Category, Supplier hepsi aynı temel işlemleri kullanır

**NEDEN `where T : BaseEntity`?**
- Sadece Entity'lerde kullanılabilir
- Tip güvenliği sağlar

### Özelleşmiş Repository'ler

```csharp
public interface IProductRepository : IRepository<Product>
{
    // Generic metodlara ek olarak özel metodlar
    Task<Product> GetByBarcodeAsync(string barcode);
    Task<IReadOnlyList<Product>> GetCriticalStockProductsAsync();
    Task DecreaseStockAsync(int productId, int quantity);
}
```

**NEDEN Özel Interface?**
- Generic repository her entity için yetmez
- Her entity'nin kendine özgü iş mantığı var

---

## 🎯 SERVICE LAYER

### Repository vs Service

| **Repository** | **Service** |
|---|---|
| Database işlemleri | İş mantığı koordinasyonu |
| Entity ile çalışır | DTO ile çalışır |
| CRUD operasyonları | Validation, Mapping, Transaction |
| Basit sorgular | Karmaşık iş kuralları |

### Service Örneği

```csharp
public interface ISaleService
{
    // DTO alır, DTO döndürür
    Task<SaleDTO> CreateSaleAsync(CreateSaleDTO dto);
    
    // İş mantığı
    Task<bool> ConfirmPaymentAsync(int saleId);
    Task<decimal> CalculateEmployeeCommissionAsync(int employeeId);
}
```

**İŞ MANTIĞI ÖRNEKLERİ:**
1. **Satış oluşturma:**
   - Stok kontrolü yap
   - Fiyat hesaplamalarını yap (KDV, indirim)
   - Satış numarası oluştur
   - Stokları azalt
   - Email gönder

2. **Prim hesaplama:**
   - Çalışanın aylık satışlarını topla
   - Kotayı geçti mi kontrol et
   - Prim tutarını hesapla (%10)

---

## 📦 DTO (Data Transfer Objects)

### NEDEN DTO?

```
❌ KÖTÜ YÖNTEM (Entity'yi direkt UI'a gönder):
Entity → Controller → View
Problem: Navigation properties → Circular reference
Problem: Hassas bilgiler görünür (Password, Internal ID)

✅ İYİ YÖNTEM (DTO kullan):
Entity → Mapping → DTO → Controller → View
```

**Avantajları:**
1. **Güvenlik** - Hassas bilgileri gizler
2. **Performance** - Sadece gerekli alanları taşır
3. **Validation** - Data Annotations ile validasyon
4. **Flexible** - UI ihtiyacına göre şekillenir

### DTO Türleri

1. **DTO (Read)** - Veri göstermek için
   ```csharp
   public class ProductDTO
   {
       public int Id { get; set; }
       public string Name { get; set; }
       public decimal UnitPrice { get; set; }
       // Navigation property yerine sadece isim
       public string CategoryName { get; set; }
   }
   ```

2. **CreateDTO** - Yeni kayıt için
   ```csharp
   public class CreateProductDTO
   {
       // ID yok (Database oluşturuyor)
       [Required]
       public string Name { get; set; }
       
       [Range(0.01, double.MaxValue)]
       public decimal UnitPrice { get; set; }
   }
   ```

3. **UpdateDTO** - Güncelleme için
   ```csharp
   public class UpdateProductDTO
   {
       [Required]
       public int Id { get; set; } // Hangi kayıt güncellenecek?
       
       [Required]
       public string Name { get; set; }
   }
   ```

4. **ListDTO** - Liste görünümü için (Hafif)
   ```csharp
   public class ProductListDTO
   {
       // Sadece listede gösterilecek alanlar
       public int Id { get; set; }
       public string Name { get; set; }
       public decimal UnitPrice { get; set; }
   }
   ```

---

## 🔄 DEPENDENCY DIRECTION

```
┌─────────────────────────────────────┐
│  Presentation (MVC/API)             │
│  ↓ bağımlı                          │
├─────────────────────────────────────┤
│  Application (Interfaces, DTOs)     │  ← Burası!
│  ↓ bağımlı                          │
├─────────────────────────────────────┤
│  Domain (Entities, Enums)           │
└─────────────────────────────────────┘

Infrastructure → Application (Implements)
Infrastructure → Domain (Uses)
```

**ÖNEMLİ:**
- Application, Infrastructure'ı **TANIMAZ**
- Sadece Interface'leri tanımlar
- Infrastructure, Interface'leri implement eder
- Dependency Inversion Principle (SOLID)

---

## 📊 ÖRNEK İŞ AKIŞI

### Ürün Ekleme İşlemi

```
1. UI (Controller):
   CreateProductDTO oluştur → Service'e gönder

2. Service Layer:
   - DTO'yu validate et
   - DTO'dan Entity'e dönüştür (Mapping)
   - İş kurallarını uygula (Stok status hesapla)
   - Repository'yi çağır

3. Repository Layer:
   - Entity'yi database'e ekle
   - SaveChangesAsync

4. Service Layer:
   - Entity'den DTO'ya dönüştür
   - DTO'yu döndür

5. UI (Controller):
   - DTO'yu View'a gönder
```

---

## 🎯 SOLID PRENSİPLERİ

### 1. Single Responsibility (SRP)
- Her service tek bir sorumluluğa sahip
- ProductService sadece ürün işlemleri
- SaleService sadece satış işlemleri

### 2. Open/Closed (OCP)
- Interface'ler değişmez (Closed)
- Implementasyon genişletilebilir (Open)

### 3. Liskov Substitution (LSP)
- IRepository<Product> kullanılan her yerde
- ProductRepository kullanılabilir

### 4. Interface Segregation (ISP)
- Büyük interface'ler yerine küçük, odaklı interface'ler
- IProductRepository sadece ürün metodları içerir

### 5. Dependency Inversion (DIP)
- Üst seviye (Service) alt seviyeye (Repository) bağımlı değil
- Her ikisi de interface'e bağımlı

---

## 🔧 KULLANILAN PAKETLER

### FluentValidation
```csharp
// Data Annotations'dan daha güçlü validasyon
public class CreateProductDTOValidator : AbstractValidator<CreateProductDTO>
{
    public CreateProductDTOValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı zorunludur")
            .MaximumLength(200);
            
        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır");
    }
}
```

### AutoMapper
```csharp
// Entity <-> DTO dönüşümleri otomatik
CreateMap<Product, ProductDTO>()
    .ForMember(dest => dest.CategoryName, 
               opt => opt.MapFrom(src => src.Category.Name));
```

---

## 📝 SONRAKI ADIM

Application Layer tamamlandı! ✅

**Sırada ne var?**
1. **Infrastructure Layer** - DbContext, Repository implementasyonları
2. **Presentation Layer** - MVC Controllers, Views
# TEKNOROMA - INFRASTRUCTURE LAYER

## 📋 GENEL BAKIŞ

Infrastructure Layer, **dışsal bağımlılıkları** yönetir. Application katmanında tanımlanan interface'leri implement eder.

### ✅ Infrastructure Layer Özellikleri:
- **Domain ve Application'a bağımlıdır**
- **Database işlemlerini yönetir** (Entity Framework Core)
- **Repository interface'lerini implement eder**
- **Dışsal servisleri entegre eder** (Email, SMS, File Storage)

### 🎯 Sorumlulukları:
1. **DbContext** - Entity Framework Core yapılandırması
2. **Repository Implementasyonları** - CRUD işlemleri
3. **Fluent API Configurations** - Database yapılandırmaları
4. **Migrations** - Database şema yönetimi

---

## 📁 KLASÖR YAPISI

```
Infrastructure/
├── Persistence/                # Database Context
│   └── TeknoromaDbContext.cs
│
├── Repositories/               # Repository Implementasyonları
│   ├── BaseRepository.cs               # Generic base implementation
│   ├── ProductRepository.cs            # Ürün işlemleri
│   ├── SaleRepository.cs               # Satış işlemleri
│   ├── OtherRepositories.cs            # Customer, Employee, vb.
│   └── TransactionRepositories.cs      # Expense, TechnicalService, vb.
│
├── Configurations/             # Fluent API Configurations
│   ├── ProductConfigurations.cs        # Product, Category, Supplier
│   └── EntityConfigurations.cs         # Diğer entity'ler
│
└── Infrastructure.csproj
```

---

## 🗄️ DbContext (TeknoromaDbContext)

### NEDEN IdentityDbContext?

```csharp
public class TeknoromaDbContext : IdentityDbContext
```

**Avantajları:**
- ASP.NET Identity tabloları otomatik oluşturulur
- User, Role, UserRole, UserLogin, UserClaim tabloları hazır
- Authentication/Authorization hazır

### DbSets (Tablolar)

Her DbSet bir database tablosuna karşılık gelir:

```csharp
public DbSet<Store> Stores { get; set; }
public DbSet<Product> Products { get; set; }
public DbSet<Sale> Sales { get; set; }
// ... 12 tablo daha
```

### OnModelCreating

Fluent API konfigürasyonlarını uygular:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Identity tablolarını oluştur
    base.OnModelCreating(modelBuilder);
    
    // Configuration'ları otomatik uygula
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(TeknoromaDbContext).Assembly);
    
    // Global Query Filters (Soft Delete)
    modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
}
```

### SaveChanges Override

Her kayıt işleminde otomatik işlemler yapar:

```csharp
public override Task<int> SaveChangesAsync(...)
{
    foreach (var entry in ChangeTracker.Entries<BaseEntity>())
    {
        switch (entry.State)
        {
            case EntityState.Added:
                entry.Entity.CreatedDate = DateTime.Now;
                break;
                
            case EntityState.Modified:
                entry.Entity.ModifiedDate = DateTime.Now;
                break;
                
            case EntityState.Deleted:
                // Soft Delete
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                break;
        }
    }
    
    return base.SaveChangesAsync();
}
```

**NEDEN Override?**
1. **Audit** - CreatedDate, ModifiedDate otomatik
2. **Soft Delete** - Fiziksel silme yerine IsDeleted = true
3. **Business Rules** - Kayıt öncesi kontroller

---

## 📦 Repository Pattern Implementation

### BaseRepository<T>

Generic repository implementation:

```csharp
public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly TeknoromaDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    // CRUD Operations
    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public virtual async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    
    // ... diğer metodlar
}
```

**NEDEN Abstract?**
- Direkt instance oluşturulmaz
- Sadece miras alınır
- Protected members: Alt sınıflara erişim

**NEDEN Virtual?**
- Alt sınıf override edebilir
- Include eklemek için
- Özel logic eklemek için

### Özelleşmiş Repository'ler

ProductRepository örneği:

```csharp
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(TeknoromaDbContext context) : base(context) { }
    
    public async Task<Product> GetByBarcodeAsync(string barcode)
    {
        return await _dbSet
            .Include(p => p.Category)      // Eager Loading
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Barcode == barcode);
    }
    
    public async Task DecreaseStockAsync(int productId, int quantity)
    {
        var product = await GetByIdAsync(productId);
        
        if (product.UnitsInStock < quantity)
            throw new Exception("Stok yetersiz!");
            
        product.UnitsInStock -= quantity;
        await UpdateStockStatusAsync(productId);
    }
}
```

**ÖNEMLİ NOKTALAR:**
1. **Include** - Eager Loading için
2. **Business Logic** - Repository'de basit logic olabilir
3. **Exception Handling** - İş kuralı ihlallerinde exception fırlat

---

## 🔧 Fluent API Configurations

### NEDEN Fluent API?

**Data Annotations vs Fluent API:**

```csharp
// ❌ Data Annotations (Entity'de):
public class Product
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; }
}

// ✅ Fluent API (Configuration'da):
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
    }
}
```

**Avantajlar:**
1. **Separation of Concerns** - Entity temiz kalır
2. **Daha güçlü** - Karmaşık konfigürasyonlar yapılabilir
3. **Merkezi yönetim** - Tüm database yapılandırmaları tek yerde

### Configuration Örneği

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Table Name
        builder.ToTable("Products");
        
        // Primary Key
        builder.HasKey(p => p.ID);
        
        // Properties
        builder.Property(p => p.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
            
        // Unique Index
        builder.HasIndex(p => p.Barcode)
            .IsUnique();
            
        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Indexes (Performance)
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.StockStatus);
    }
}
```

### Delete Behaviors

```csharp
// Restrict: Parent silinirse hata ver
.OnDelete(DeleteBehavior.Restrict)

// Cascade: Parent silinirse child da silinsin
.OnDelete(DeleteBehavior.Cascade)

// SetNull: Parent silinirse FK = NULL yap
.OnDelete(DeleteBehavior.SetNull)
```

**TEKNOROMA'da:**
- Çoğunlukla **Restrict** kullanıyoruz
- Sadece SaleDetail için **Cascade** (Sale silinirse detail de silinir)

---

## 🗄️ MIGRATIONS

### Migration Nedir?

Migration, database şemasını **kod olarak** yönetir:
- Version control ile takip edilir
- Geri alınabilir
- Takım çalışmasında tutarlılık

### Migration Komutları

#### 1. İlk Migration Oluşturma

```powershell
# Package Manager Console (Visual Studio):
Add-Migration InitialCreate -Project Infrastructure -StartupProject MVC

# .NET CLI:
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project MVC
```

**NE YAPAR?**
- `Migrations/` klasöründe C# dosyaları oluşturur
- `Up()`: Database oluşturma
- `Down()`: Geri alma

#### 2. Database Oluşturma

```powershell
# Package Manager Console:
Update-Database -Project Infrastructure -StartupProject MVC

# .NET CLI:
dotnet ef database update --project Infrastructure --startup-project MVC
```

**NE YAPAR?**
- SQL Server'da database oluşturur
- Tabloları oluşturur
- İlişkileri kurar

#### 3. Yeni Migration Ekleme

```powershell
# Yeni bir sütun ekledik, migration oluştur:
Add-Migration AddProductImageUrl -Project Infrastructure -StartupProject MVC
```

#### 4. Migration Geri Alma

```powershell
# Son migration'ı geri al:
Update-Database -Migration PreviousMigration -Project Infrastructure

# Belirli bir migration'a geri dön:
Update-Database -Migration AddNewFeature -Project Infrastructure
```

#### 5. Migration Silme

```powershell
# Henüz uygulanmamış son migration'ı sil:
Remove-Migration -Project Infrastructure
```

### Connection String

`appsettings.json` (MVC projesinde):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TeknoromaDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**ÖNEMLİ NOTLAR:**
- `Server=localhost`: Local SQL Server
- `Trusted_Connection=True`: Windows Authentication
- `TrustServerCertificate=True`: SSL sertifika kontrolünü atla

---

## 🔍 LINQ Sorgular

### Basic Queries

```csharp
// Single Entity
var product = await _dbSet.FindAsync(id);

// Filter
var products = await _dbSet
    .Where(p => p.UnitPrice > 1000)
    .ToListAsync();

// Order By
var products = await _dbSet
    .OrderByDescending(p => p.CreatedDate)
    .ToListAsync();

// Pagination
var products = await _dbSet
    .Skip(20)   // İlk 20'yi atla
    .Take(10)   // Sonraki 10'u al
    .ToListAsync();
```

### Include (Eager Loading)

```csharp
// Include: İlişkili tabloları getir
var product = await _dbSet
    .Include(p => p.Category)
    .Include(p => p.Supplier)
    .FirstOrDefaultAsync(p => p.ID == id);

// ThenInclude: İç içe ilişkiler
var sale = await _dbSet
    .Include(s => s.SaleDetails)
        .ThenInclude(sd => sd.Product)
            .ThenInclude(p => p.Category)
    .FirstOrDefaultAsync(s => s.ID == id);
```

### Aggregations

```csharp
// Count
var count = await _dbSet.CountAsync();

// Sum
var total = await _dbSet.SumAsync(p => p.UnitPrice);

// Average
var avg = await _dbSet.AverageAsync(p => p.UnitPrice);

// Max / Min
var max = await _dbSet.MaxAsync(p => p.UnitPrice);
```

### GroupBy

```csharp
// En çok satan ürünler
var topProducts = await _context.SaleDetails
    .GroupBy(sd => sd.Product)
    .Select(g => new
    {
        Product = g.Key,
        TotalQuantity = g.Sum(sd => sd.Quantity)
    })
    .OrderByDescending(x => x.TotalQuantity)
    .Take(10)
    .ToListAsync();
```

---

## 📊 ER DIAGRAM (Database Schema)

```
┌─────────────┐         ┌──────────────┐         ┌──────────────┐
│   Stores    │1──────N │ Departments  │N──────1 │  Employees   │
└─────────────┘         └──────────────┘         └──────────────┘
      │1                                                │1
      │N                                                │N
┌─────────────┐         ┌──────────────┐         ┌──────────────┐
│   Sales     │N──────1 │  Customers   │         │  Products    │
└─────────────┘         └──────────────┘         └──────────────┘
      │1                                                │N
      │N                                                │1
┌─────────────┐         ┌──────────────┐         ┌──────────────┐
│ SaleDetails │N──────1 │  Products    │1──────N │ Categories   │
└─────────────┘         └──────────────┘         └──────────────┘
                              │N                        
                              │1                        
                        ┌──────────────┐
                        │  Suppliers   │
                        └──────────────┘
```

---

## 🎯 BEST PRACTICES

### 1. Async/Await Kullan

```csharp
// ✅ GOOD
public async Task<Product> GetByIdAsync(int id)
{
    return await _dbSet.FindAsync(id);
}

// ❌ BAD
public Product GetById(int id)
{
    return _dbSet.Find(id);  // Senkron - UI donar
}
```

### 2. IReadOnlyList Kullan

```csharp
// ✅ GOOD - Sadece okuma
public async Task<IReadOnlyList<Product>> GetAllAsync()
{
    return await _dbSet.ToListAsync();
}

// ❌ BAD - Liste değiştirilebilir
public async Task<List<Product>> GetAllAsync()
{
    return await _dbSet.ToListAsync();
}
```

### 3. AsNoTracking Kullan (Read-Only İşlemler)

```csharp
// Sadece okuma yapıyorsak (Update yapmayacaksak)
var products = await _dbSet
    .AsNoTracking()  // Daha hızlı - ChangeTracker kullanmaz
    .ToListAsync();
```

### 4. Include Dikkatli Kullan

```csharp
// ❌ BAD - Gereksiz veri yükleme
var product = await _dbSet
    .Include(p => p.Category)
    .Include(p => p.Supplier)
    .Include(p => p.SaleDetails)  // Gereksiz!
    .FirstOrDefaultAsync(p => p.ID == id);

// ✅ GOOD - Sadece gerekli olanlar
var product = await _dbSet
    .Include(p => p.Category)
    .Include(p => p.Supplier)
    .FirstOrDefaultAsync(p => p.ID == id);
```

### 5. Global Query Filter'ı Unutma

```csharp
// Soft Delete filter'ı atlamak için:
var allProducts = await _dbSet
    .IgnoreQueryFilters()  // IsDeleted = true olanları da getir
    .ToListAsync();
```

---

## 📝 SONRAKI ADIM

Infrastructure Layer tamamlandı! ✅

**Sırada ne var?**
1. **Presentation Layer - MVC** - Controllers, Views
2. **Presentation Layer - API** - API Controllers, Swagger
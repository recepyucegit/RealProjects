# Domain Layer

Domain katmanı, Onion Architecture'ın en iç katmanıdır. Hiçbir dış bağımlılığı yoktur ve iş mantığının temelini oluşturur.

## Proje Bilgileri

- **Framework:** .NET 8.0
- **Nullable:** Enabled
- **Bağımlılıklar:** Yok (saf C# sınıfları)

## Klasör Yapısı

```
Domain/
├── Entities/          # Veritabanı tablolarını temsil eden sınıflar
│   ├── BaseEntity.cs  # Tüm entity'lerin temel sınıfı
│   ├── Store.cs       # Mağaza
│   ├── Department.cs  # Departman
│   ├── Employee.cs    # Çalışan
│   ├── Customer.cs    # Müşteri
│   ├── Category.cs    # Kategori
│   ├── Supplier.cs    # Tedarikçi
│   ├── Product.cs     # Ürün
│   ├── Sale.cs        # Satış
│   ├── SaleDetail.cs  # Satış Detayı
│   ├── Expense.cs     # Gider
│   ├── TechnicalService.cs    # Teknik Servis
│   └── SupplierTransaction.cs # Tedarikçi Hareketi
└── Enums/             # Sabit değer listeleri
    ├── UserRole.cs
    ├── Gender.cs
    ├── StockStatus.cs
    ├── SaleStatus.cs
    ├── PaymentType.cs
    ├── ExpenseType.cs
    ├── Currency.cs
    └── TechnicalServiceStatus.cs
```

## Entity'ler

### BaseEntity
Tüm entity'lerin miras aldığı temel sınıf.

| Property | Tip | Açıklama |
|----------|-----|----------|
| Id | int | Primary Key (Auto Increment) |
| CreatedDate | DateTime | Kayıt oluşturulma tarihi |
| ModifiedDate | DateTime? | Son güncelleme tarihi |
| IsDeleted | bool | Soft Delete için |

### Store (Mağaza)
TEKNOROMA'nın 55 mağazasını temsil eder.

| Property | Tip | Açıklama |
|----------|-----|----------|
| Name | string | Mağaza adı |
| City | string | Şehir |
| District | string | İlçe |
| Address | string | Adres |
| Phone | string | Telefon |
| Email | string | Email |
| IsActive | bool | Aktif mi? |

**Navigation Properties:** Employees, Departments, Sales, Expenses, TechnicalServices

### Department (Departman)
Mağaza içindeki departmanlar.

| Property | Tip | Açıklama |
|----------|-----|----------|
| Name | string | Departman adı |
| Description | string? | Açıklama |
| StoreId | int | Mağaza FK |
| DepartmentType | UserRole | Departman türü |

**Navigation Properties:** Store, Employees

### Employee (Çalışan)
Toplam 258 çalışan.

| Property | Tip | Açıklama |
|----------|-----|----------|
| IdentityUserId | string | ASP.NET Identity ID |
| IdentityNumber | string | TC Kimlik No (UNIQUE) |
| FirstName | string | Ad |
| LastName | string | Soyad |
| Email | string | Email |
| Phone | string | Telefon |
| BirthDate | DateTime | Doğum tarihi |
| HireDate | DateTime | İşe giriş tarihi |
| Salary | decimal | Maaş |
| StoreId | int | Mağaza FK |
| DepartmentId | int | Departman FK |
| Role | UserRole | Kullanıcı rolü |
| SalesQuota | decimal? | Satış kotası |
| IsActive | bool | Aktif mi? |

**Calculated:** FullName
**Navigation Properties:** Store, Department, Sales, Expenses, AssignedTechnicalServices

### Customer (Müşteri)

| Property | Tip | Açıklama |
|----------|-----|----------|
| IdentityNumber | string | TC Kimlik No (UNIQUE) |
| FirstName | string | Ad |
| LastName | string | Soyad |
| BirthDate | DateTime? | Doğum tarihi |
| Gender | Gender? | Cinsiyet |
| Email | string? | Email |
| Phone | string | Telefon |
| Address | string? | Adres |
| City | string? | Şehir |
| IsActive | bool | Aktif mi? |

**Calculated:** FullName, Age
**Navigation Properties:** Sales

### Category (Kategori)
Ürün kategorileri.

| Property | Tip | Açıklama |
|----------|-----|----------|
| Name | string | Kategori adı |
| Description | string? | Açıklama |
| IsActive | bool | Aktif mi? |

**Navigation Properties:** Products

### Supplier (Tedarikçi)

| Property | Tip | Açıklama |
|----------|-----|----------|
| CompanyName | string | Firma adı |
| ContactName | string | Yetkili kişi |
| Phone | string | Telefon |
| Email | string | Email |
| Address | string? | Adres |
| City | string? | Şehir |
| Country | string | Ülke (default: Türkiye) |
| TaxNumber | string | Vergi no |
| IsActive | bool | Aktif mi? |

**Navigation Properties:** Products, SupplierTransactions

### Product (Ürün)

| Property | Tip | Açıklama |
|----------|-----|----------|
| Name | string | Ürün adı |
| Description | string | Açıklama |
| Barcode | string | Barkod (UNIQUE) |
| UnitPrice | decimal | Birim fiyat |
| UnitsInStock | int | Stok miktarı |
| CriticalStockLevel | int | Kritik stok seviyesi |
| StockStatus | StockStatus | Stok durumu |
| CategoryId | int | Kategori FK |
| SupplierId | int | Tedarikçi FK |
| IsActive | bool | Aktif mi? |
| ImageUrl | string? | Görsel URL |

**Calculated:** StockStatusText, IsAvailable
**Navigation Properties:** Category, Supplier, SaleDetails, SupplierTransactions

### Sale (Satış)

| Property | Tip | Açıklama |
|----------|-----|----------|
| SaleNumber | string | Satış no (S-2024-00001) |
| SaleDate | DateTime | Satış tarihi |
| CustomerId | int | Müşteri FK |
| EmployeeId | int | Çalışan FK |
| StoreId | int | Mağaza FK |
| Status | SaleStatus | Satış durumu |
| PaymentType | PaymentType | Ödeme türü |
| Subtotal | decimal | Ara toplam |
| TaxAmount | decimal | KDV |
| DiscountAmount | decimal | İndirim |
| TotalAmount | decimal | Toplam |
| CashRegisterNumber | string? | Kasa no |
| Notes | string? | Notlar |

**Navigation Properties:** Customer, Employee, Store, SaleDetails

### SaleDetail (Satış Detayı)

| Property | Tip | Açıklama |
|----------|-----|----------|
| SaleId | int | Satış FK |
| ProductId | int | Ürün FK |
| ProductName | string | Ürün adı (snapshot) |
| UnitPrice | decimal | Birim fiyat |
| Quantity | int | Miktar |
| DiscountPercentage | decimal | İndirim % |
| Subtotal | decimal | Ara toplam |
| DiscountAmount | decimal | İndirim tutarı |
| TotalAmount | decimal | Toplam |

**Navigation Properties:** Sale, Product

### Expense (Gider)

| Property | Tip | Açıklama |
|----------|-----|----------|
| ExpenseNumber | string | Gider no (G-2024-00001) |
| ExpenseDate | DateTime | Gider tarihi |
| ExpenseType | ExpenseType | Gider türü |
| StoreId | int | Mağaza FK |
| EmployeeId | int? | Çalışan FK (maaş için) |
| Amount | decimal | Tutar |
| Currency | Currency | Para birimi |
| ExchangeRate | decimal? | Döviz kuru |
| AmountInTRY | decimal | TL karşılığı |
| Description | string | Açıklama |
| DocumentNumber | string? | Evrak no |
| IsPaid | bool | Ödendi mi? |
| PaymentDate | DateTime? | Ödeme tarihi |

**Navigation Properties:** Store, Employee

### TechnicalService (Teknik Servis)

| Property | Tip | Açıklama |
|----------|-----|----------|
| ServiceNumber | string | Servis no (TS-2024-00001) |
| Title | string | Başlık |
| Description | string | Açıklama |
| StoreId | int | Mağaza FK |
| ReportedByEmployeeId | int | Bildiren çalışan FK |
| AssignedToEmployeeId | int? | Atanan çalışan FK |
| IsCustomerIssue | bool | Müşteri sorunu mu? |
| CustomerId | int? | Müşteri FK |
| Status | TechnicalServiceStatus | Durum |
| Priority | int | Öncelik (1-4) |
| ReportedDate | DateTime | Bildirim tarihi |
| ResolvedDate | DateTime? | Çözüm tarihi |
| Resolution | string? | Çözüm açıklaması |

**Navigation Properties:** Store, ReportedByEmployee, AssignedToEmployee, Customer

### SupplierTransaction (Tedarikçi Hareketi)

| Property | Tip | Açıklama |
|----------|-----|----------|
| TransactionNumber | string | İşlem no (TH-2024-00001) |
| TransactionDate | DateTime | İşlem tarihi |
| SupplierId | int | Tedarikçi FK |
| ProductId | int | Ürün FK |
| Quantity | int | Miktar |
| UnitPrice | decimal | Birim fiyat |
| TotalAmount | decimal | Toplam |
| InvoiceNumber | string? | Fatura no |
| Notes | string? | Notlar |
| IsPaid | bool | Ödendi mi? |
| PaymentDate | DateTime? | Ödeme tarihi |

**Navigation Properties:** Supplier, Product

## Enum'lar

### UserRole
```csharp
GenelMudur = 1      // Tüm sisteme erişim
SubeMuduru = 2      // Kendi şubesinin işlemleri
KasaSatis = 3       // Satış işlemleri
Depo = 4            // Stok ve ürün işlemleri
Muhasebe = 5        // Finansal işlemler
TeknikServis = 6    // Servis kayıtları
```

### Gender
```csharp
Erkek = 1
Kadin = 2
Belirtilmemis = 3
```

### StockStatus
```csharp
Yeterli = 1
Kritik = 2
Tukendi = 3
```

### SaleStatus
```csharp
Beklemede = 1       // Ödeme bekleniyor
Hazirlaniyor = 2    // Ürünler hazırlanıyor
Tamamlandi = 3      // Satış tamamlandı
Iptal = 4           // İptal edildi
```

### PaymentType
```csharp
Nakit = 1
KrediKarti = 2
BankaKarti = 3
Havale = 4
Cek = 5
```

### ExpenseType
```csharp
CalisanOdemesi = 1        // Maaş ödemeleri
TeknikAltyapiGideri = 2   // Sunucu, yazılım, bakım
Fatura = 3                // Elektrik, su, doğalgaz
DigerGider = 4            // Diğer
```

### Currency
```csharp
TRY = 1
USD = 2
EUR = 3
```

### TechnicalServiceStatus
```csharp
Acik = 1        // Henüz işleme alınmadı
Islemde = 2     // Üzerinde çalışılıyor
Tamamlandi = 3  // Çözüldü
Cozulemedi = 4  // Çözülemedi
```

## İlişki Diyagramı

```
Store (1) ──────┬──── (*) Department
                ├──── (*) Employee
                ├──── (*) Sale
                ├──── (*) Expense
                └──── (*) TechnicalService

Department (1) ──── (*) Employee

Employee (1) ──────┬──── (*) Sale
                   ├──── (*) Expense
                   └──── (*) TechnicalService (Assigned)

Customer (1) ──────┬──── (*) Sale
                   └──── (*) TechnicalService

Category (1) ──── (*) Product

Supplier (1) ──────┬──── (*) Product
                   └──── (*) SupplierTransaction

Product (1) ───────┬──── (*) SaleDetail
                   └──── (*) SupplierTransaction

Sale (1) ──── (*) SaleDetail
```

## Kullanım

Bu katman sadece POCO (Plain Old CLR Objects) sınıfları içerir. Veritabanı işlemleri Infrastructure katmanında, iş mantığı Application katmanında yer alır.

```csharp
// Entity oluşturma örneği
var product = new Product
{
    Name = "iPhone 15 Pro",
    Description = "Apple iPhone 15 Pro 256GB",
    Barcode = "1234567890123",
    UnitPrice = 65000,
    UnitsInStock = 50,
    CategoryId = 1,
    SupplierId = 1
};
```

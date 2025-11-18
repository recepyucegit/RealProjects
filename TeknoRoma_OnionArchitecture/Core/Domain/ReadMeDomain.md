# TEKNOROMA - DOMAIN LAYER

## 📋 GENEL BAKIŞ

Domain Layer, Onion Architecture'ın **merkezi**dir. Tüm iş mantığı ve kuralları burada tanımlanır.

### ✅ Domain Layer Özellikleri:
- **Hiçbir dış bağımlılığı yok** (Pure C#)
- **Entity Framework'e bağımlı değil**
- **Database'e bağımlı değil**
- **UI'a bağımlı değil**

### 🎯 Sorumlulukları:
1. Entity'leri tanımlar (Database tablolarını temsil eder)
2. Enum'ları tanımlar (Sabit değerler)
3. İş kurallarını tanımlar

---

## 📁 KLASÖR YAPISI

```
Domain/
├── Entities/           # Database tablolarını temsil eden sınıflar
│   ├── BaseEntity.cs           # Tüm entity'lerin miras aldığı temel sınıf
│   ├── Store.cs                # Mağaza
│   ├── Department.cs           # Departman
│   ├── Employee.cs             # Çalışan
│   ├── Customer.cs             # Müşteri
│   ├── Supplier.cs             # Tedarikçi
│   ├── Category.cs             # Kategori
│   ├── Product.cs              # Ürün
│   ├── Sale.cs                 # Satış (Başlık)
│   ├── SaleDetail.cs           # Satış Detayı (Satırlar)
│   ├── SupplierTransaction.cs  # Tedarikçi Hareketi
│   ├── Expense.cs              # Gider
│   └── TechnicalService.cs     # Teknik Servis
│
└── Enums/              # Sabit değerler
    ├── UserRole.cs                 # Kullanıcı Rolleri
    ├── SaleStatus.cs               # Satış Durumu
    ├── PaymentType.cs              # Ödeme Türü
    ├── ExpenseType.cs              # Gider Türü
    ├── TechnicalServiceStatus.cs   # Teknik Servis Durumu
    ├── Gender.cs                   # Cinsiyet
    ├── Currency.cs                 # Para Birimi
    └── StockStatus.cs              # Stok Durumu
```

---

## 🏗️ ENTITY İLİŞKİLERİ

### Store (Mağaza)
- **1 → N**: Employees (Çalışanlar)
- **1 → N**: Departments (Departmanlar)
- **1 → N**: Sales (Satışlar)
- **1 → N**: Expenses (Giderler)
- **1 → N**: TechnicalServices (Teknik Servisler)

### Department (Departman)
- **N → 1**: Store (Mağaza)
- **1 → N**: Employees (Çalışanlar)

### Employee (Çalışan)
- **N → 1**: Store (Mağaza)
- **N → 1**: Department (Departman)
- **1 → N**: Sales (Satışlar)
- **1 → N**: Expenses (Maaş Ödemeleri)
- **1 → N**: TechnicalServices (Çözülen Servisler)

### Customer (Müşteri)
- **1 → N**: Sales (Satışlar)
- **1 → N**: TechnicalServices (Müşteri Servisleri)

### Supplier (Tedarikçi)
- **1 → N**: Products (Ürünler)
- **1 → N**: SupplierTransactions (Alım Hareketleri)

### Category (Kategori)
- **1 → N**: Products (Ürünler)

### Product (Ürün)
- **N → 1**: Category (Kategori)
- **N → 1**: Supplier (Tedarikçi)
- **1 → N**: SaleDetails (Satış Detayları)
- **1 → N**: SupplierTransactions (Alım Hareketleri)

### Sale (Satış)
- **N → 1**: Customer (Müşteri)
- **N → 1**: Employee (Çalışan)
- **N → 1**: Store (Mağaza)
- **1 → N**: SaleDetails (Satış Detayları)

### SaleDetail (Satış Detayı) - **Junction Table**
- **N → 1**: Sale (Satış)
- **N → 1**: Product (Ürün)

### SupplierTransaction (Tedarikçi Hareketi)
- **N → 1**: Supplier (Tedarikçi)
- **N → 1**: Product (Ürün)

### Expense (Gider)
- **N → 1**: Store (Mağaza)
- **N → 1**: Employee (Çalışan - Opsiyonel)

### TechnicalService (Teknik Servis)
- **N → 1**: Store (Mağaza)
- **N → 1**: Employee (Bildiren)
- **N → 1**: Employee (Çözen - Opsiyonel)
- **N → 1**: Customer (Müşteri - Opsiyonel)

---

## 🎯 ÖNEMLİ KAVRAMLAR

### 1️⃣ BaseEntity
Tüm entity'ler BaseEntity'den miras alır.
**Ortak Özellikler:**
- ID (Primary Key)
- CreatedDate (Oluşturulma tarihi)
- ModifiedDate (Güncellenme tarihi)
- IsDeleted (Soft Delete)

### 2️⃣ Navigation Properties
Entity Framework ilişkileri için kullanılır.
**virtual** keyword: Lazy Loading için

### 3️⃣ Foreign Keys
İlişkili tablonun ID'sini tutar.
Örn: `ProductId`, `CategoryId`, `SupplierId`

### 4️⃣ Calculated Properties
Database'e kaydedilmez, runtime'da hesaplanır.
Örn: `FullName`, `Age`, `StockStatusText`

### 5️⃣ Soft Delete
Kayıt fiziksel olarak silinmez, `IsDeleted = true` yapılır.
**Avantajları:**
- Raporlarda geçmiş verilere erişim
- Yanlışlıkla silinen kayıtları geri getirme
- Audit trail (İz sürme)

---

## 📊 DATABASE DİYAGRAMI (ER Diagram)

```
┌─────────────┐         ┌──────────────┐         ┌──────────────┐
│   Store     │1──────N │ Department   │N──────1 │  Employee    │
│             │         │              │         │              │
│ ID          │         │ StoreId (FK) │         │ DepartmentId │
│ Name        │         │ Name         │         │ FirstName    │
│ City        │         └──────────────┘         │ Salary       │
└─────────────┘                                  │ Role         │
      │                                          └──────────────┘
      │                                                │
      │1                                               │1
      │                                                │
      │N                                               │N
┌─────────────┐         ┌──────────────┐         ┌──────────────┐
│    Sale     │N──────1 │   Customer   │         │   Product    │
│             │         │              │         │              │
│ SaleNumber  │         │ IdentityNo   │         │ Name         │
│ CustomerId  │         │ FirstName    │         │ UnitPrice    │
│ EmployeeId  │         │ BirthDate    │         │ UnitsInStock │
│ TotalAmount │         │ Gender       │         └──────────────┘
└─────────────┘         └──────────────┘               │N
      │1                                                │
      │                                                 │1
      │N                                          ┌─────────────┐
┌─────────────┐         ┌──────────────┐         │  Category   │
│ SaleDetail  │N──────1 │   Product    │1──────N │             │
│             │         │              │         │ Name        │
│ SaleId (FK) │         │ CategoryId   │         │ Description │
│ ProductId   │         │ SupplierId   │         └─────────────┘
│ Quantity    │         └──────────────┘
│ UnitPrice   │               │N
└─────────────┘               │
                              │1
                        ┌─────────────┐
                        │  Supplier   │
                        │             │
                        │ CompanyName │
                        │ ContactName │
                        └─────────────┘
```

---

## 🔄 SONRAKI ADIM

Domain Layer tamamlandı! ✅

**Sırada ne var?**
1. **Application Layer** - Repository interface'leri, Service interface'leri, DTO'lar
2. **Infrastructure Layer** - DbContext, Repository implementasyonları
3. **Presentation Layer** - MVC Controllers, Views

---

## 📝 NOTLAR

- Domain katmanı **hiçbir şeye bağımlı değil** - Bu çok önemli!
- Enum'lar database'de **integer** olarak saklanır
- Navigation Properties **virtual** - Lazy Loading için
- BaseEntity **abstract** - Direkt instance oluşturulmaz
- Soft Delete kullanılıyor - **IsDeleted** property
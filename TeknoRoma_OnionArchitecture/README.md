# TeknoRoma Elektronik Mağazalar Zinciri - ERP Sistemi

![.NET](https://img.shields.io/badge/.NET-7.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-11.0-239120?logo=csharp)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap)
![License](https://img.shields.io/badge/License-MIT-green)

**Bitirme Projesi** - Onion Architecture ile geliştirilmiş, profesyonel bir kurumsal kaynak planlama (ERP) sistemi.

---

## 📋 İçindekiler

- [Proje Hakkında](#-proje-hakkında)
- [Mimari](#-mimari)
- [Teknoloji Stack](#-teknoloji-stack)
- [Özellikler](#-özellikler)
- [Kurulum](#-kurulum)
- [Kullanım](#-kullanım)
- [API Dokümantasyonu](#-api-dokümantasyonu)
- [Demo Kullanıcılar](#-demo-kullanıcılar)
- [Proje Yapısı](#-proje-yapısı)
- [Ekran Görüntüleri](#-ekran-görüntüleri)

---

## 🎯 Proje Hakkında

TeknoRoma, Türkiye genelinde **55 mağazası** (İstanbul 15, İzmir 15, Ankara 15, Bursa 10) bulunan bir elektronik mağazalar zinciridir. Bu proje, mağaza operasyonlarını yönetmek için geliştirilmiş kapsamlı bir ERP sistemidir.

### Proje Kapsamı

- **Satış Yönetimi:** POS sistemi, sepet mantığı, ödeme takibi
- **Stok Yönetimi:** Gerçek zamanlı stok takibi, kritik seviye uyarıları
- **İnsan Kaynakları:** Çalışan yönetimi, prim hesaplama
- **Muhasebe:** Gider takibi, çok para birimi desteği
- **Teknik Servis:** Sorun takibi, öncelik bazlı yönetim
- **Raporlama:** Kapsamlı satış ve performans raporları

### İş Gereksinimleri

Sistem, 6 farklı rol için özelleştirilmiş fonksiyonlar sunar:

1. **Şube Müdürü (Haluk Bey)** - Mağaza geneli raporlar ve analiz
2. **Kasa Satış Temsilcisi (Gül Satar)** - POS satış işlemleri
3. **Mobil Satış Temsilcisi (Fahri Cepçi)** - Mobil satış, barkod tarama
4. **Depo Temsilcisi (Kerim Zulacı)** - Stok ve sipariş yönetimi
5. **Muhasebe Temsilcisi (Feyza Paragöz)** - Gider ve ödeme takibi
6. **Teknik Servis Temsilcisi (Özgün Kablocu)** - Arıza ve sorun yönetimi

---

## 🏗️ Mimari

Proje, **Onion Architecture** (Soğan Mimarisi) prensiplerine göre tasarlanmıştır. Bu mimari, bağımlılıkların dıştan içe doğru olmasını sağlar ve iş mantığının altyapı detaylarından bağımsız olmasını garanti eder.

```
┌─────────────────────────────────────────────────────┐
│                 Presentation Layer                  │
│  ┌──────────────────┐      ┌──────────────────┐    │
│  │   Web (MVC)      │      │   Web.API        │    │
│  │  - Areas (6)     │      │  - REST Endpoints│    │
│  │  - Controllers   │      │  - JWT Auth      │    │
│  │  - Views         │      │  - Swagger UI    │    │
│  └──────────────────┘      └──────────────────┘    │
└─────────────┬───────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────┐
│              Application Layer                      │
│  - IUnitOfWork (Repository Pattern)                │
│  - IRepository Interfaces                           │
│  - Business Logic Contracts                         │
└─────────────┬───────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────┐
│                 Domain Layer (Core)                 │
│  - Entities (13 models)                            │
│  - Enums (PaymentType, Currency, IssueStatus, etc.)│
│  - Business Rules                                   │
│  - No External Dependencies                         │
└─────────────┬───────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────┐
│              Infrastructure Layer                   │
│  - ApplicationDbContext (EF Core)                  │
│  - Repository Implementations (11 repos)            │
│  - UnitOfWork Implementation                        │
│  - Entity Configurations (Fluent API)               │
│  - Database Migrations                              │
└─────────────────────────────────────────────────────┘
```

### Katmanlar ve Sorumluluklar

#### 1. Domain (Core) Layer
- **Bağımlılığı yok** - En iç katman
- Entities: Product, Customer, Sale, Employee, vb.
- Enums ve business rules
- Tüm katmanlar Domain'e bağımlıdır

#### 2. Application Layer
- Domain'e bağımlı
- Repository interfaces (IProductRepository, ISaleRepository, vb.)
- IUnitOfWork pattern
- Business logic contracts

#### 3. Infrastructure Layer
- Application ve Domain'e bağımlı
- EF Core DbContext
- Repository implementations
- Database configurations
- External service integrations

#### 4. Presentation Layer
- Tüm katmanlara bağımlı
- **Web (MVC):** 6 Area, Controllers, Views
- **Web.API:** RESTful endpoints, Swagger, JWT
- User interface ve API endpoints

---

## 🚀 Teknoloji Stack

### Backend
- **Framework:** ASP.NET Core 7.0 (MVC + Web API)
- **ORM:** Entity Framework Core 7.0
- **Database:** SQL Server 2022
- **Authentication:** ASP.NET Identity + JWT Bearer
- **API Documentation:** Swashbuckle (Swagger/OpenAPI)
- **Logging:** Serilog
- **Validation:** FluentValidation
- **Mapping:** AutoMapper

### Frontend
- **UI Framework:** Bootstrap 5.3
- **Icons:** Bootstrap Icons
- **Charts:** Chart.js
- **JavaScript:** Vanilla JS (ES6+)

### Design Patterns
- **Repository Pattern** - Data access abstraction
- **Unit of Work Pattern** - Transaction management
- **Dependency Injection** - IoC container
- **Soft Delete Pattern** - Logical deletion
- **Snapshot Pattern** - Historical data preservation

---

## ✨ Özellikler

### 1. Rol Bazlı Yetkilendirme

6 farklı rol, her biri kendi Area'sı ile:

#### **Şube Müdürü (SubeYoneticisi)**
- Dashboard: Günlük/Aylık satışlar, çalışan sayısı, kritik stok
- Satış raporları (tarih aralığı)
- En çok satan 10 ürün analizi
- Çalışan performansı ve prim hesaplaması
- Tüm raporlara erişim

#### **Kasa Satış (KasaSatis)**
- POS satış ekranı (sepet mantığı)
- TC Kimlik ile müşteri arama
- Barkod okutma
- Otomatik KDV hesaplama (%20)
- İndirim uygulama
- Ödeme türü seçimi (Nakit, Kredi Kartı, Havale, Çek)
- Prim ve kota takibi
- Otomatik stok azaltma

#### **Mobil Satış (MobilSatis)**
- Mobil cihazdan satış
- Barkod tarama
- Stok kontrolü
- Prim hesaplama
- Kasa satış ile benzer yetkiler

#### **Depo (Depo)**
- Kritik stok listesi (≤10 adet)
- Stokta olmayan ürünler
- Bekleyen siparişler
- Sipariş hazırlama
- Stok güncelleme
- Durum değiştirme (Hazırlanıyor → Tamamlandı)

#### **Muhasebe (Muhasebe)**
- Gider girişi
- Çok para birimi desteği (TRY, USD, EUR)
- Döviz kuru otomasyonu
- Ödenmemiş faturalar
- Aylık gider raporları
- Ödeme kaydı

#### **Teknik Servis (TeknikServis)**
- Yeni sorun kaydı
- Öncelik seviyesi (1-Düşük, 4-Kritik)
- Müşteri/Sistem sorunları
- Teknisyene atama
- Açık sorunlar listesi
- Sorun çözme ve kapama

### 2. İş Mantığı Özellikleri

#### Satış İşlemleri
- **Sepet Mantığı:** Dinamik sepet, ürün ekleme/çıkarma
- **KDV Hesaplama:** Otomatik %20 KDV
- **İndirim Sistemi:** Esnek indirim uygulaması
- **Satış Numarası:** Otomatik unique numara (S-2024-00001)
- **Stok Kontrolü:** Satış sırasında yetersiz stok uyarısı
- **Transaction Safety:** Tüm satış işlemleri transaction içinde

#### Prim Hesaplama
```csharp
// İş Kuralı:
// - Satış Kotası: 10,000 TL/ay
// - Prim Oranı: %10
// - Sadece kotayı aşan tutar primlendiri lir

Prim = (Toplam Satış - Kota) * 0.10

// Örnek:
// Aylık Satış: 15,000 TL
// Kota: 10,000 TL
// Prim: (15,000 - 10,000) * 0.10 = 500 TL
```

#### Stok Yönetimi
```csharp
Stok Durumu:
- Tükendi: UnitsInStock = 0
- Kritik: UnitsInStock <= 10
- Yeterli: UnitsInStock > 10

// Otomatik güncelleme:
// - Satış → Stok azaltma
// - Sipariş gelişi → Stok artırma
// - Her değişiklikte StockStatus otomatik güncellenir
```

#### Döviz Sistemi
- **Desteklenen:** TRY, USD, EUR
- **Döviz Kuru:** Manuel girişdesteklenir
- **Otomatik Dönüşüm:** Tüm tutarlar TL'ye çevrilir
```csharp
AmountInTRY = Amount * ExchangeRate

// Örnek:
// 100 USD * 32.50 kur = 3,250 TL
```

### 3. Güvenlik

- **Authentication:** ASP.NET Identity (Cookie-based, 8 saat)
- **Authorization:** Role-based ([Authorize(Roles = "...")])
- **JWT Tokens:** API için 480 dakika geçerlilik
- **Lockout:** 5 hatalı denemede hesap kilidi
- **Password Policy:** Büyük/küçük harf, rakam, özel karakter
- **SQL Injection:** Parametreli sorgular (EF Core)
- **XSS Protection:** Razor encoding
- **CORS:** API için yapılandırılabilir

---

## 📦 Kurulum

### Gereksinimler

- **.NET SDK 7.0** veya üstü
- **SQL Server 2019** veya üstü (LocalDB, Express, Developer, Standard)
- **Visual Studio 2022** veya **Visual Studio Code**
- **Git** (opsiyonel)

### Adım 1: Projeyi Klonlama

```bash
git clone https://github.com/your-username/TeknoRoma_OnionArchitecture.git
cd TeknoRoma_OnionArchitecture
```

### Adım 2: Connection String Ayarlama

**Web/appsettings.json** ve **Web.API/appsettings.json** dosyalarını düzenleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TeknoRomaDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

**LocalDB için:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TeknoRomaDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Adım 3: NuGet Paketlerini Yükleme

```bash
dotnet restore
```

### Adım 4: Veritabanı Oluşturma

**Migration oluşturma:**
```bash
dotnet ef migrations add InitialCreate --project Infrastructure/Infrastructure.csproj --startup-project Presentation/Web/Web.csproj
```

**Veritabanını güncelleme:**
```bash
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project Presentation/Web/Web.csproj
```

### Adım 5: Uygulamayı Çalıştırma

#### MVC Uygulaması:
```bash
cd Presentation/Web
dotnet run
```
Tarayıcıda: `https://localhost:5001`

#### Web API:
```bash
cd Presentation/Web.API
dotnet run
```
Swagger UI: `https://localhost:5201` (port numarası değişebilir)

### Adım 6: Demo Veriler

Uygulama ilk çalıştırıldığında **SeedData** otomatik olarak çalışır ve:
- 6 rol oluşturur
- 6 demo kullanıcı oluşturur (her rolden birer tane)

---

## 👥 Demo Kullanıcılar

Tüm kullanıcıların şifresi: **TeknoRoma123!**

| Rol | İsim | Email | Kullanım Amacı |
|-----|------|-------|----------------|
| **Şube Müdürü** | Haluk Bey | halukbey@teknoroma.com | Raporlar, çalışan performansı |
| **Kasa Satış** | Gül Satar | gulsatar@teknoroma.com | POS satış işlemleri |
| **Mobil Satış** | Fahri Cepçi | fahricepci@teknoroma.com | Mobil satış, barkod |
| **Depo** | Kerim Zulacı | kerimzulaci@teknoroma.com | Stok yönetimi |
| **Muhasebe** | Feyza Paragöz | feyzaparagoz@teknoroma.com | Gider takibi |
| **Teknik Servis** | Özgün Kablocu | ozgunkablocu@teknoroma.com | Sorun takibi |

---

## 💻 Kullanım

### MVC Uygulaması

1. **Giriş Yapın:** `https://localhost:5001`
2. **Email ve Şifre** girin (yukarıdaki tabloya bakın)
3. **Role göre dashboard** açılır
4. **Menüden** işlemlerinizi yapın

#### Örnek: Satış Yapma (Gül Satar)

1. Email: `gulsatar@teknoroma.com`, Şifre: `TeknoRoma123!`
2. Dashboard → **Yeni Satış** butonuna tıklayın
3. **TC Kimlik** ile müşteri arayın
4. **Barkod** okutarak veya ürün adı yazarak ürün ekleyin
5. **Miktar** belirleyin, **Sepete Ekle**
6. **Ödeme türü** seçin (Nakit, Kredi Kartı, vb.)
7. İndirim varsa girin
8. **Satışı Tamamla** butonuna tıklayın
9. Stok otomatik azalır, satış kaydedilir

#### Örnek: Kritik Stok Kontrolü (Kerim Zulacı)

1. Email: `kerimzulaci@teknoroma.com`
2. Dashboard → **Kritik Stok** butonuna tıklayın
3. 10 adet ve altındaki ürünler listelenir
4. **Stok Güncelle** veya **Sipariş Ver** yapabilirsiniz

### Web API

#### Swagger UI'da Test Etme:

1. `https://localhost:5201` adresine gidin
2. **Authorize** butonuna tıklayın
3. Önce **login** endpointini çağırın:

```bash
POST /api/auth/login
{
  "email": "halukbey@teknoroma.com",
  "password": "TeknoRoma123!"
}
```

4. Dönen **token**'ı kopyalayın
5. **Authorize** penceresine `Bearer <token>` şeklinde yapıştırın
6. Artık korumalı endpointleri kullanabilirsiniz

---

## 🔌 API Dokümantasyonu

### Authentication

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "halukbey@teknoroma.com",
  "password": "TeknoRoma123!"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "halukbey@teknoroma.com",
  "username": "halukbey"
}
```

### Products

#### Get All Products
```http
GET /api/products
Authorization: Bearer <token>

Response: 200 OK
[
  {
    "id": 1,
    "name": "iPhone 15 Pro",
    "barcode": "1234567890123",
    "unitPrice": 45000.00,
    "unitsInStock": 25,
    "stockStatus": "Yeterli"
  }
]
```

#### Search by Barcode
```http
GET /api/products/barcode/1234567890123
Authorization: Bearer <token>

Response: 200 OK
{
  "id": 1,
  "name": "iPhone 15 Pro",
  "category": {
    "name": "Akıllı Telefonlar"
  },
  "unitPrice": 45000.00,
  "unitsInStock": 25
}
```

#### Get Critical Stock (Depo veya SubeYoneticisi only)
```http
GET /api/products/critical-stock
Authorization: Bearer <token>

Response: 200 OK
[
  {
    "id": 5,
    "name": "Samsung Galaxy S23",
    "unitsInStock": 8,
    "stockStatus": "Kritik"
  }
]
```

### Sales

#### Create Sale
```http
POST /api/sales
Authorization: Bearer <token>
Content-Type: application/json

{
  "customerId": 1,
  "employeeId": 2,
  "paymentType": "KrediKarti",
  "discountAmount": 500.00,
  "items": [
    {
      "productId": 1,
      "quantity": 2,
      "unitPrice": 45000.00
    },
    {
      "productId": 3,
      "quantity": 1,
      "unitPrice": 12000.00
    }
  ]
}

Response: 201 Created
{
  "id": 42,
  "saleNumber": "S-2024-00042",
  "subtotal": 102000.00,
  "taxAmount": 20400.00,
  "discountAmount": 500.00,
  "totalAmount": 121900.00
}
```

#### Get Pending Orders (Depo role)
```http
GET /api/sales/pending
Authorization: Bearer <token>

Response: 200 OK
[
  {
    "id": 10,
    "saleNumber": "S-2024-00010",
    "status": "Hazirlaniyor",
    "totalAmount": 75000.00,
    "saleDate": "2024-11-20T10:30:00"
  }
]
```

### Customers

#### Search by Identity Number
```http
GET /api/customers/identity/12345678901
Authorization: Bearer <token>

Response: 200 OK
{
  "id": 5,
  "firstName": "Ahmet",
  "lastName": "Yılmaz",
  "identityNumber": "12345678901",
  "phone": "05551234567",
  "email": "ahmet@example.com"
}
```

### Expenses

#### Create Expense (Muhasebe only)
```http
POST /api/expenses
Authorization: Bearer <token>
Content-Type: application/json

{
  "storeId": 1,
  "expenseType": "Fatura",
  "description": "Elektrik faturası - Kasım 2024",
  "amount": 15000.00,
  "currency": "TRY",
  "exchangeRate": 1.0,
  "expenseDate": "2024-11-20"
}

Response: 201 Created
```

#### Get Unpaid Expenses
```http
GET /api/expenses/unpaid
Authorization: Bearer <token>

Response: 200 OK
[
  {
    "id": 3,
    "description": "Kira - Kasım",
    "amountInTRY": 50000.00,
    "expenseDate": "2024-11-01",
    "isPaid": false
  }
]
```

### Technical Services

#### Create Issue (TeknikServis role)
```http
POST /api/technicalservices
Authorization: Bearer <token>
Content-Type: application/json

{
  "storeId": 1,
  "customerId": 5,
  "issueType": "Musteri",
  "title": "iPhone ekran arızası",
  "description": "Ekran yanıp sönüyor",
  "priority": 3
}

Response: 201 Created
```

#### Get Open Issues
```http
GET /api/technicalservices/open
Authorization: Bearer <token>

Response: 200 OK
[
  {
    "id": 7,
    "title": "iPhone ekran arızası",
    "priority": 3,
    "status": "Acik",
    "reportedDate": "2024-11-20T14:30:00"
  }
]
```

### Employees

#### Get Performance (SubeYoneticisi role)
```http
GET /api/employees/2/performance/2024/11
Authorization: Bearer <token>

Response: 200 OK
{
  "employeeId": 2,
  "employeeName": "Gül Satar",
  "year": 2024,
  "month": 11,
  "totalSales": 125000.00,
  "salesQuota": 100000.00,
  "commission": 2500.00,
  "quotaPercentage": 125.0
}
```

---

## 📁 Proje Yapısı

```
TeknoRoma_OnionArchitecture/
│
├── Domain/                                    # Core Layer (No Dependencies)
│   ├── Entities/
│   │   ├── BaseEntity.cs                     # Base entity (ID, dates, IsDeleted)
│   │   ├── Product.cs                        # Ürün
│   │   ├── Category.cs                       # Kategori
│   │   ├── Supplier.cs                       # Tedarikçi
│   │   ├── Store.cs                          # Mağaza (55 şube)
│   │   ├── Department.cs                     # Departman
│   │   ├── Employee.cs                       # Çalışan (Identity entegrasyonu)
│   │   ├── Customer.cs                       # Müşteri
│   │   ├── Sale.cs                           # Satış
│   │   ├── SaleDetail.cs                     # Satış detayı
│   │   ├── Expense.cs                        # Gider
│   │   ├── SupplierTransaction.cs            # Tedarikçi işlemi
│   │   └── TechnicalService.cs               # Teknik servis kaydı
│   └── Enums/
│       ├── StockStatus.cs                    # Tükendi, Kritik, Yeterli
│       ├── PaymentType.cs                    # Nakit, KrediKarti, Havale, Cek
│       ├── SaleStatus.cs                     # Hazirlaniyor, Tamamlandi, Iptal
│       ├── Currency.cs                       # TRY, USD, EUR
│       ├── ExpenseType.cs                    # CalisanOdemesi, TeknikAltyapi, Fatura
│       ├── IssueType.cs                      # Musteri, Sistem
│       ├── IssueStatus.cs                    # Acik, Devam, Cozuldu, Kapandi
│       └── Gender.cs                         # Erkek, Kadin, Diger
│
├── Application/                               # Application Layer
│   └── Repositories/
│       ├── IRepository.cs                    # Generic repository interface
│       ├── IUnitOfWork.cs                    # Unit of Work pattern
│       ├── IProductRepository.cs             # Product-specific methods
│       ├── ISaleRepository.cs                # Sale-specific methods
│       ├── ICustomerRepository.cs
│       ├── IEmployeeRepository.cs
│       ├── IExpenseRepository.cs
│       ├── ITechnicalServiceRepository.cs
│       └── ... (11 repositories total)
│
├── Infrastructure/                            # Infrastructure Layer
│   ├── Context/
│   │   └── ApplicationDbContext.cs           # EF Core DbContext
│   ├── Configurations/                       # Fluent API configurations
│   │   ├── ProductConfiguration.cs
│   │   ├── SaleConfiguration.cs
│   │   ├── CustomerConfiguration.cs
│   │   └── ... (13 configuration files)
│   ├── Repositories/                         # Repository implementations
│   │   ├── Repository.cs                     # Generic repository (17 methods)
│   │   ├── UnitOfWork.cs                     # Transaction support
│   │   ├── ProductRepository.cs
│   │   ├── SaleRepository.cs
│   │   └── ... (11 repository implementations)
│   ├── DependencyInjection.cs                # Service registration
│   └── Migrations/                           # EF Core migrations
│
├── Presentation/
│   ├── Web/                                  # MVC Application
│   │   ├── Program.cs                        # Application entry point
│   │   ├── SeedData.cs                       # Demo users and roles
│   │   ├── appsettings.json
│   │   │
│   │   ├── Controllers/
│   │   │   ├── BaseController.cs             # Base for all controllers
│   │   │   └── AccountController.cs          # Login/Logout
│   │   │
│   │   ├── Models/
│   │   │   ├── Account/
│   │   │   │   └── LoginViewModel.cs
│   │   │   ├── Sale/
│   │   │   │   └── SaleCreateViewModel.cs
│   │   │   ├── Product/
│   │   │   │   └── ProductSearchViewModel.cs
│   │   │   ├── Customer/
│   │   │   │   └── CustomerSearchViewModel.cs
│   │   │   └── Report/
│   │   │       ├── DashboardViewModel.cs
│   │   │       └── ProductSalesReportViewModel.cs
│   │   │
│   │   ├── Areas/
│   │   │   ├── SubeYoneticisi/
│   │   │   │   ├── Controllers/
│   │   │   │   │   ├── DashboardController.cs
│   │   │   │   │   └── ReportController.cs
│   │   │   │   └── Views/
│   │   │   │       ├── Dashboard/Index.cshtml
│   │   │   │       └── Report/TopProducts.cshtml
│   │   │   │
│   │   │   ├── KasaSatis/
│   │   │   │   ├── Controllers/
│   │   │   │   │   ├── DashboardController.cs
│   │   │   │   │   ├── SaleController.cs
│   │   │   │   │   └── CustomerController.cs
│   │   │   │   └── Views/
│   │   │   │       ├── Dashboard/Index.cshtml
│   │   │   │       └── Sale/Create.cshtml      # Shopping cart UI
│   │   │   │
│   │   │   ├── MobilSatis/
│   │   │   ├── Depo/
│   │   │   │   └── Views/
│   │   │   │       └── Stock/Critical.cshtml   # Critical stock list
│   │   │   ├── Muhasebe/
│   │   │   │   └── Views/
│   │   │   │       └── Expense/Create.cshtml   # Expense entry form
│   │   │   └── TeknikServis/
│   │   │       └── Views/
│   │   │           └── Issue/Create.cshtml     # Issue reporting
│   │   │
│   │   ├── Views/
│   │   │   ├── Shared/
│   │   │   │   └── _Layout.cshtml             # Bootstrap 5 layout
│   │   │   ├── Account/
│   │   │   │   └── Login.cshtml
│   │   │   ├── _ViewImports.cshtml
│   │   │   └── _ViewStart.cshtml
│   │   │
│   │   └── wwwroot/
│   │       └── css/
│   │           └── site.css                    # Custom styles
│   │
│   └── Web.API/                               # REST API
│       ├── Program.cs                         # JWT + Swagger configuration
│       ├── appsettings.json                   # JWT settings
│       └── Controllers/
│           ├── AuthController.cs              # JWT login
│           ├── ProductsController.cs          # Products CRUD
│           ├── SalesController.cs             # Sales operations
│           ├── CustomersController.cs         # Customer management
│           ├── ExpensesController.cs          # Expense tracking
│           ├── TechnicalServicesController.cs # Issue management
│           └── EmployeesController.cs         # Employee & performance
│
└── README.md                                  # This file
```

---

## 📸 Ekran Görüntüleri

### Login Sayfası
- Email ve şifre girişi
- Demo kullanıcılar listesi
- Bootstrap 5 modern tasarım

### Şube Müdürü Dashboard
- Bugünkü satış: XX,XXX TL
- Aylık satış: XXX,XXX TL
- Toplam çalışan: XX kişi
- Kritik stok: X ürün
- Hızlı erişim butonları: Raporlar, Çalışan Performansı

### Kasa Satış - Yeni Satış
- Sol panel: Müşteri seçimi (TC Kimlik arama), Ödeme bilgileri
- Sağ panel: Barkod okutma, Sepet, Özet (Ara toplam, KDV, İndirim, Toplam)
- Dinamik JavaScript sepet yönetimi

### Depo - Kritik Stok
- Tablo: Barkod, Ürün, Kategori, Tedarikçi, Stok, Durum
- Renk kodları: Kırmızı (Tükendi), Sarı (Kritik)
- Stok güncelleme modal
- İstatistikler: Stokta yok, Kritik seviye, Toplam değer

### Muhasebe - Gider Girişi
- Gider türü seçimi
- Çok para birimi (TRY, USD, EUR)
- Otomatik döviz kuru hesaplama
- Ödendi checkbox → Ödeme tarihi
- Bilgilendirme kutuları

### Teknik Servis - Sorun Kaydı
- Sorun türü (Müşteri / Sistem)
- Öncelik seviyesi (1-4, dinamik bilgilendirme)
- Müşteri arama (sadece Müşteri sorunlarında)
- Teknisyen atama
- Öncelik rehberi tablosu

### API - Swagger UI
- Authorize butonu (JWT Bearer)
- Tüm endpointler listeleniyor
- Try it out ile test
- Request/Response örnekleri

---

## 📊 Veritabanı Şeması

### Ana Tablolar

| Tablo | Açıklama | İlişkiler |
|-------|----------|-----------|
| **Products** | Ürün bilgileri | → Categories, Suppliers |
| **Sales** | Satış başlığı | → Customers, Employees, Stores, SaleDetails |
| **SaleDetails** | Satış detayları | → Sales, Products |
| **Customers** | Müşteri bilgileri | TC Kimlik unique |
| **Employees** | Çalışan bilgileri | → Stores, Departments, Identity Users |
| **Stores** | Mağaza bilgileri | 55 şube (İstanbul, İzmir, Ankara, Bursa) |
| **Expenses** | Gider kayıtları | → Stores |
| **TechnicalServices** | Teknik servis | → Stores, Customers, Employees |

### Önemli Alanlar

**Products:**
- `Barcode` (UNIQUE, Index)
- `StockStatus` (Enum: Tükendi, Kritik, Yeterli)
- `UnitsInStock` → Otomatik StockStatus güncelleme

**Sales:**
- `SaleNumber` (UNIQUE, Format: S-YYYY-XXXXX)
- `Subtotal`, `TaxAmount` (%20), `DiscountAmount`, `TotalAmount`
- `PaymentType`, `Status`

**SaleDetails:**
- `ProductName`, `UnitPrice` (Snapshot - fiyat değişse de tarihsel kayıt korunur)

**Employees:**
- `IdentityUserId` (ASP.NET Identity entegrasyonu)
- `IdentityNumber` (UNIQUE, 11 haneli TC Kimlik)
- `SalesQuota` (Prim hesaplamabakınız için)

**Expenses:**
- `Currency` (TRY, USD, EUR)
- `ExchangeRate`, `AmountInTRY`

**TechnicalServices:**
- `Priority` (1-4)
- `IssueType` (Müşteri, Sistem)
- `Status` (Açık, Devam, Çözüldü, Kapandı)

---

## 🔐 Güvenlik Notları

### Production Ortamı İçin

1. **JWT Secret:** `appsettings.json` içindeki `JwtSettings:Secret` değerini güçlü bir key ile değiştirin
2. **Connection String:** Production veritabanı bilgilerini güvenli bir şekilde saklayın (Azure Key Vault, vb.)
3. **HTTPS:** Production'da mutlaka HTTPS kullanın
4. **CORS:** `AllowAll` yerine sadece güvenilir origin'lere izin verin
5. **Rate Limiting:** API için rate limiting ekleyin
6. **Logging:** Hassas bilgileri loglara yazmayın
7. **Validation:** Tüm input'ları validate edin (FluentValidation kullanılıyor)

---

## 🧪 Test

### Unit Test Örneği (Gelecek Geliştirme)

```bash
# Test projesi ekleme
dotnet new xunit -n TeknoRoma.Tests
dotnet add TeknoRoma.Tests reference Application/Application.csproj

# Testleri çalıştırma
dotnet test
```

### API Test (Postman Collection)

Postman collection hazırlanabilir:
- Login endpoint
- Tüm CRUD işlemleri
- Role-based authorization testleri

---

## 📈 Performans İyileştirmeleri

### Uygulanmış:
- **Lazy Loading:** UnitOfWork'te repository'ler lazy initialize ediliyor
- **Eager Loading:** İlgili entity'ler `Include()` ile yükleniyor
- **Indexing:** Barcode, TC Kimlik, SaleNumber gibi alanlarda unique index
- **Pagination:** Büyük listelerde (gelecek geliştirme)
- **Caching:** (Gelecek geliştirme - Redis)

### Öneriler:
- Response caching eklenebilir
- Redis distributed cache
- CDN kullanımı (static dosyalar için)
- Database query optimization

---

## 🚧 Bilinen Sorunlar ve Gelecek Geliştirmeler

### Gelecek Özellikler:
- [ ] Excel/PDF export (raporlar için)
- [ ] Email notifications (sipariş, kritik stok)
- [ ] Real-time notifications (SignalR)
- [ ] Mobile app (Xamarin/MAUI)
- [ ] Dashboard charts (daha fazla grafik)
- [ ] Advanced search ve filtering
- [ ] Audit logging (kim, ne zaman, ne değiştirdi)
- [ ] Multi-tenancy (farklı şirketler için)
- [ ] Integration tests
- [ ] CI/CD pipeline (GitHub Actions, Azure DevOps)

### Bilinen Sorunlar:
- Excel export henüz implement edilmedi
- Bazı view'larda pagination yok (gelecek sürümde)

---

## 👨‍💻 Geliştirici

**Bitirme Projesi**
- Mimari: Onion Architecture
- Proje Tipi: Kurumsal ERP Sistemi
- Yıl: 2024

---

## 📄 Lisans

Bu proje [MIT](https://opensource.org/licenses/MIT) lisansı altında lisanslanmıştır.

---

## 🙏 Teşekkürler

- **ASP.NET Core Team** - Framework
- **Entity Framework Core Team** - ORM
- **Bootstrap Team** - UI Framework
- **Chart.js** - Grafik kütüphanesi

---

## 📞 İletişim ve Destek

Sorularınız veya önerileriniz için:
- **GitHub Issues:** [Proje Issues Sayfası](https://github.com/your-username/TeknoRoma_OnionArchitecture/issues)
- **Email:** your-email@example.com

---

## 🎓 Eğitim Amaçlı Not

Bu proje, **Onion Architecture** prensiplerini göstermek ve kurumsal bir ERP sisteminin nasıl tasarlanacağını öğretmek amacıyla geliştirilmiştir. Clean Architecture, SOLID prensipleri, Design Patterns (Repository, Unit of Work, Dependency Injection) gibi konuları pratik olarak uygulamaktadır.

**Öğrenilen Konular:**
- Onion/Clean Architecture
- Repository ve Unit of Work Pattern
- ASP.NET Core MVC (Areas, Controllers, Views)
- ASP.NET Core Web API (RESTful)
- Entity Framework Core (Code-First, Fluent API)
- ASP.NET Identity (Authentication & Authorization)
- JWT Bearer Authentication
- Dependency Injection
- Soft Delete Pattern
- Transaction Management
- Role-Based Authorization

---

**⭐ Bu proje işinize yaradıysa, lütfen bir yıldız bırakın!**

---

*Son güncelleme: Kasım 2024*

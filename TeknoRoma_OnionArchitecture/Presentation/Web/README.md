# TeknoRoma MVC Presentation Layer

## 📋 Proje Hakkında

TeknoRoma Elektronik Mağazalar Zinciri için geliştirilmiş ASP.NET Core 7.0 MVC uygulaması.

**Bitirme Projesi** - Onion Architecture ile geliştirilmiş profesyonel bir ERP sistemi.

---

## 🎯 Özellikler

### 1. **Role-Based Authentication & Authorization**
- ASP.NET Identity entegrasyonu
- 6 farklı rol için özelleştirilmiş dashboard'lar
- Cookie-based authentication (8 saatlik oturum)
- Lockout koruması (5 hatalı denemede hesap kilitlenir)

### 2. **Areas Yapısı**
Her rol için ayrı Area:
- **SubeYoneticisi** - Şube Müdürü (Haluk Bey)
- **KasaSatis** - Kasa Satış Temsilcisi (Gül Satar)
- **MobilSatis** - Mobil Satış Temsilcisi (Fahri Cepçi)
- **Depo** - Depo Temsilcisi (Kerim Zulacı)
- **Muhasebe** - Muhasebe Temsilcisi (Feyza Paragöz)
- **TeknikServis** - Teknik Servis Temsilcisi (Özgün Kablocu)

### 3. **İş Mantığı**
- **Satış İşlemleri:** Sepet mantığı, otomatik KDV hesaplama (%20), indirim desteği
- **Stok Yönetimi:** Otomatik stok azaltma, kritik seviye uyarıları
- **Prim Hesaplama:** Satış kotası (10.000 TL), %10 prim oranı
- **Gider Takibi:** Döviz kuru desteği (TRY, USD, EUR)
- **Teknik Servis:** Öncelik bazlı sorun takibi (1-Düşük, 4-Kritik)

---

## 👥 Demo Kullanıcılar

| Rol | Kullanıcı | Email | Şifre |
|-----|-----------|-------|-------|
| Şube Müdürü | Haluk Bey | halukbey@teknoroma.com | TeknoRoma123! |
| Kasa Satış | Gül Satar | gulsatar@teknoroma.com | TeknoRoma123! |
| Mobil Satış | Fahri Cepçi | fahricepci@teknoroma.com | TeknoRoma123! |
| Depo | Kerim Zulacı | kerimzulaci@teknoroma.com | TeknoRoma123! |
| Muhasebe | Feyza Paragöz | feyzaparagoz@teknoroma.com | TeknoRoma123! |
| Teknik Servis | Özgün Kablocu | ozgunkablocu@teknoroma.com | TeknoRoma123! |

---

## 🏗️ Proje Yapısı

```
Presentation/Web/
├── Program.cs                          # Uygulama giriş noktası
├── SeedData.cs                         # Demo kullanıcılar ve roller
├── appsettings.json                    # Konfigürasyon
├── Web.csproj                          # Proje dosyası
│
├── Controllers/
│   ├── BaseController.cs               # Temel controller (UnitOfWork, Logger)
│   └── AccountController.cs            # Login/Logout
│
├── Models/
│   ├── Account/
│   │   └── LoginViewModel.cs
│   ├── Sale/
│   │   └── SaleCreateViewModel.cs      # Satış + Sepet mantığı
│   ├── Product/
│   │   └── ProductSearchViewModel.cs   # Ürün arama (Barkod vb.)
│   └── Report/
│       └── DashboardViewModel.cs       # Dashboard verileri
│
├── Areas/
│   ├── SubeYoneticisi/
│   │   ├── Controllers/
│   │   │   ├── DashboardController.cs  # Genel raporlar
│   │   │   └── ReportController.cs     # Satış, ürün, çalışan raporları
│   │   └── Views/
│   │       └── Dashboard/Index.cshtml
│   │
│   ├── KasaSatis/
│   │   ├── Controllers/
│   │   │   ├── DashboardController.cs  # Prim hesaplama
│   │   │   ├── SaleController.cs       # Yeni satış, barkod okutma
│   │   │   └── CustomerController.cs   # TC Kimlik ile arama
│   │   └── Views/
│   │       └── Dashboard/Index.cshtml
│   │
│   ├── MobilSatis/
│   │   ├── Controllers/
│   │   │   └── DashboardController.cs
│   │   └── Views/
│   │
│   ├── Depo/
│   │   ├── Controllers/
│   │   │   ├── DashboardController.cs
│   │   │   └── StockController.cs      # Kritik stok, bekleyen siparişler
│   │   └── Views/
│   │
│   ├── Muhasebe/
│   │   ├── Controllers/
│   │   │   ├── DashboardController.cs
│   │   │   └── ExpenseController.cs    # Gider girişi, ödeme takibi
│   │   └── Views/
│   │
│   └── TeknikServis/
│       ├── Controllers/
│       │   ├── DashboardController.cs
│       │   └── IssueController.cs      # Sorun kaydı, öncelik sıralaması
│       └── Views/
│
├── Views/
│   ├── _ViewImports.cshtml             # Using'ler
│   ├── _ViewStart.cshtml               # Layout tanımı
│   ├── Shared/
│   │   └── _Layout.cshtml              # Ana layout (Bootstrap 5)
│   └── Account/
│       └── Login.cshtml                # Giriş sayfası
│
└── wwwroot/
    └── css/
        └── site.css                    # Özel stiller
```

---

## 🚀 Nasıl Çalıştırılır?

### 1. **Veritabanı Hazırlığı**

```bash
# Migration oluştur
dotnet ef migrations add InitialCreate --project Infrastructure/Infrastructure.csproj --startup-project Presentation/Web/Web.csproj

# Veritabanını güncelle
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project Presentation/Web/Web.csproj
```

### 2. **Connection String Güncelleme**

`appsettings.json` dosyasında SQL Server connection string'inizi güncelleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TeknoRomaDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

### 3. **Uygulamayı Çalıştırın**

```bash
cd Presentation/Web
dotnet run
```

**Tarayıcıda:** `https://localhost:5001`

### 4. **Giriş Yapın**

Yukarıdaki demo kullanıcı bilgilerini kullanarak giriş yapın.

---

## 📊 Roller ve Yetkiler

### **1. Şube Müdürü (Haluk Bey)**

**Dashboard:**
- Bugünkü/Aylık satışlar
- Toplam çalışan sayısı
- Kritik stok uyarısı

**Raporlar:**
- Satış raporları (tarih aralığı)
- En çok satılan 10 ürün
- Çalışan performansı ve prim hesaplaması

**Yetkiler:**
- Tüm raporları görme
- Çalışan performansını takip etme
- Mağaza geneli analiz

---

### **2. Kasa Satış (Gül Satar)**

**Dashboard:**
- Aylık satışlar
- Satış kotası ve ilerleme çubuğu
- Prim tutarı

**İşlemler:**
- **Yeni Satış:**
  - Müşteri seç (TC Kimlik ile ara)
  - Ürün ekle (Barkod okutma)
  - İndirim uygula
  - Ödeme türü seç (Nakit, Kredi Kartı, Havale, Çek)
  - Otomatik KDV hesaplama (%20)
  - Stok otomatik azalır

- **Müşteri Yönetimi:**
  - TC Kimlik No ile arama
  - Yeni müşteri kaydı

**Yetkiler:**
- Satış yapma
- Müşteri kaydı
- Kendi satışlarını görme

---

### **3. Mobil Satış (Fahri Cepçi)**

**Dashboard:**
- Aylık satışlar ve kota durumu
- Prim hesaplama

**İşlemler:**
- Barkod okutarak ürün bilgisi görme
- Stok kontrolü
- Mobil satış yapma

**Yetkiler:**
- Kasa satış ile aynı
- Mobil cihazdan satış

---

### **4. Depo (Kerim Zulacı)**

**Dashboard:**
- Bekleyen siparişler (Hazırlanıyor durumundaki)
- Kritik stok sayısı
- Stokta olmayan ürünler

**İşlemler:**
- **Stok Yönetimi:**
  - Kritik seviyedeki ürünler listesi
  - Stokta olmayan ürünler
  - Stok güncelleme

- **Sipariş:**
  - Hazırlanıyor durumundaki satışlar
  - Sipariş hazırlama
  - Durumu "Tamamlandı" yapma

**Yetkiler:**
- Stok görüntüleme ve güncelleme
- Bekleyen siparişleri hazırlama

---

### **5. Muhasebe (Feyza Paragöz)**

**Dashboard:**
- Ödenmemiş fatura sayısı ve tutarı
- Aylık toplam gider

**İşlemler:**
- **Gider Girişi:**
  - Gider türü: Çalışan Ödemesi, Teknik Altyapı, Fatura, Diğer
  - Döviz desteği (TRY, USD, EUR)
  - Döviz kuru girişi
  - Otomatik TL'ye çevirme

- **Ödeme Takibi:**
  - Ödenmemiş giderler listesi
  - Ödeme kaydetme

**Yetkiler:**
- Gider girişi
- Maaş ödemesi kaydı
- Finansal raporlar

---

### **6. Teknik Servis (Özgün Kablocu)**

**Dashboard:**
- Açık sorunlar sayısı
- Bana atanan işler
- Kritik öncelikli sorunlar

**İşlemler:**
- **Yeni Sorun Kaydı:**
  - Başlık ve açıklama
  - Sorun türü: Müşteri / Sistem
  - Öncelik: 1 (Düşük) - 4 (Kritik)
  - Müşteri bilgisi (müşteri sorunlarında)

- **Sorun Takibi:**
  - Açık sorunlar (öncelik sırasına göre)
  - Bana atananlar
  - Sorun çözme ve kapama

**Yetkiler:**
- Sorun kaydı
- Soruna atanma
- Sorun çözme

---

## ✅ Tamamlanan Özellikler

- ✅ Proje yapılandırması (Web.csproj, Program.cs)
- ✅ ASP.NET Identity entegrasyonu
- ✅ SeedData ile demo kullanıcılar
- ✅ BaseController ve AccountController
- ✅ 6 Area için controller'lar (12 controller)
- ✅ ViewModels (Login, Sale, Product, Dashboard)
- ✅ Layout ve Login view
- ✅ Örnek Dashboard view'ları (2 adet)
- ✅ CSS stilleri (Bootstrap 5)

---

## 📝 Eksik Kalan View'lar

Aşağıdaki view'lar oluşturulmalıdır:

### **SubeYoneticisi Area:**
- ✅ Dashboard/Index.cshtml
- ❌ Report/Sales.cshtml
- ❌ Report/TopProducts.cshtml
- ❌ Report/EmployeePerformance.cshtml

### **KasaSatis Area:**
- ✅ Dashboard/Index.cshtml
- ❌ Sale/Create.cshtml (Sepet mantığı)
- ❌ Customer/Search.cshtml

### **MobilSatis Area:**
- ❌ Dashboard/Index.cshtml
- ❌ Sale/Create.cshtml
- ❌ Product/Search.cshtml (Barkod okutma)

### **Depo Area:**
- ❌ Dashboard/Index.cshtml
- ❌ Stock/Critical.cshtml
- ❌ Stock/OutOfStock.cshtml
- ❌ Order/Pending.cshtml

### **Muhasebe Area:**
- ❌ Dashboard/Index.cshtml
- ❌ Expense/Create.cshtml
- ❌ Expense/Unpaid.cshtml

### **TeknikServis Area:**
- ❌ Dashboard/Index.cshtml
- ❌ Issue/Open.cshtml
- ❌ Issue/Create.cshtml

---

## 🔧 Nasıl View Oluşturulur?

### Örnek: Sale/Create.cshtml

```cshtml
@model SaleCreateViewModel
@{
    ViewData["Title"] = "Yeni Satış";
}

<h2><i class="bi bi-cart-plus"></i> Yeni Satış</h2>

<form method="post" asp-action="Create">
    <div class="row">
        <div class="col-md-6">
            <div class="card">
                <div class="card-header">
                    <h5>Müşteri Bilgileri</h5>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label asp-for="CustomerId" class="form-label"></label>
                        <input asp-for="CustomerId" class="form-control" />
                        <span asp-validation-for="CustomerId" class="text-danger"></span>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-md-6">
            <div class="card">
                <div class="card-header">
                    <h5>Ödeme Bilgileri</h5>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label asp-for="PaymentType" class="form-label"></label>
                        <select asp-for="PaymentType" class="form-select" asp-items="Html.GetEnumSelectList<PaymentType>()"></select>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="row mt-3">
        <div class="col">
            <button type="submit" class="btn btn-primary">
                <i class="bi bi-check-circle"></i> Satışı Tamamla
            </button>
        </div>
    </div>
</form>
```

---

## 📚 Teknolojiler

- **Framework:** ASP.NET Core 7.0 MVC
- **Authentication:** ASP.NET Identity
- **ORM:** Entity Framework Core 7.0
- **UI:** Bootstrap 5 + Bootstrap Icons
- **Logging:** Serilog
- **Mapping:** AutoMapper
- **Validation:** FluentValidation

---

## 🎓 Proje Mimarisi

```
┌─────────────────────────────────────┐
│   Presentation (MVC - Areas)        │
│   - Controllers                     │
│   - Views                           │
│   - ViewModels                      │
└────────────┬────────────────────────┘
             │
             ▼
┌─────────────────────────────────────┐
│   Application (Business Logic)      │
│   - Service Interfaces              │
│   - DTOs                            │
│   - Repository Interfaces           │
└────────────┬────────────────────────┘
             │
             ▼
┌─────────────────────────────────────┐
│   Domain (Core)                     │
│   - Entities                        │
│   - Enums                           │
│   - Business Rules                  │
└────────────┬────────────────────────┘
             │
             ▼
┌─────────────────────────────────────┐
│   Infrastructure (Data Access)      │
│   - DbContext                       │
│   - Repositories                    │
│   - UnitOfWork                      │
│   - EF Core Configurations          │
└─────────────────────────────────────┘
```

---

## 📞 İletişim

**Proje:** TeknoRoma Elektronik Mağazalar Zinciri ERP Sistemi
**Geliştirici:** Bitirme Projesi Öğrencisi
**Mimari:** Onion Architecture
**Tarih:** 2024

---

**Not:** Bu README dosyası projenin yapısını ve kullanımını açıklar. Eksik view'lar yukarıdaki örneklere benzer şekilde oluşturulabilir.

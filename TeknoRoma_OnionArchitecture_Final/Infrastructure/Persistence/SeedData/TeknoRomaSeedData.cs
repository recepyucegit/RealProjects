// ===================================================================================
// TEKNOROMA - SEED DATA GENERATOR (TeknoRomaSeedData.cs)
// ===================================================================================
//
// BU DOSYANIN AMACI:
// ------------------
// Veritabanına başlangıç test verileri eklemek için Bogus kütüphanesini kullanır.
// Gerçekçi ve tutarlı veriler oluşturarak geliştirme ve test süreçlerini hızlandırır.
//
// BOGUS KÜTÜPHANESİ:
// ------------------
// - Faker.js'in .NET versiyonu
// - Türkçe dahil 30+ dil desteği
// - Gerçekçi isim, adres, telefon, email vb. üretir
// - Deterministik: Aynı seed ile aynı verileri üretir
//
// KULLANIM:
// ---------
// AppDbContext'in OnModelCreating metodunda çağrılır:
// modelBuilder.Entity<Category>().HasData(TeknoRomaSeedData.GetCategories());
//
// ===================================================================================

using Bogus;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Persistence.SeedData
{
    /// <summary>
    /// TeknoRoma Seed Data Generator
    ///
    /// Tüm entity'ler için başlangıç verilerini oluşturur.
    /// Bogus kütüphanesi ile gerçekçi Türkçe veriler üretilir.
    /// </summary>
    public static class TeknoRomaSeedData
    {
        // ====================================================================
        // SABIT DEĞİŞKENLER - İŞ KURALLARINA GÖRE
        // ====================================================================

        // Mağaza sayıları (iş gereksinimi)
        private const int IstanbulStoreCount = 20;
        private const int IzmirStoreCount = 13;
        private const int AnkaraStoreCount = 13;
        private const int BursaStoreCount = 9;
        private const int TotalStoreCount = 55; // İstanbul(20) + İzmir(13) + Ankara(13) + Bursa(9)

        private const int TotalDepartmentCount = 30;
        private const int TotalEmployeeCount = 258;

        // Seed değeri - aynı verileri tekrar üretmek için
        private const int RandomSeed = 12345;

        // ====================================================================
        // CACHE - Veriler bir kez oluşturulup tekrar kullanılır
        // ====================================================================
        private static List<Store>? _cachedStores;
        private static List<Supplier>? _cachedSuppliers;
        private static List<Department>? _cachedDepartments;
        private static List<Customer>? _cachedCustomers;
        private static List<Employee>? _cachedEmployees;
        private static List<Product>? _cachedProducts;

        // ====================================================================
        // ID TRACKER'LAR - Foreign Key İlişkileri İçin
        // ====================================================================
        // Her entity için ID sayacı tutuyoruz çünkü Bogus rastgele ID üretirken
        // foreign key ilişkilerini doğru kurmak için kontrollü ID'ler gerekiyor

        private static int _storeIdCounter = 1;
        private static int _departmentIdCounter = 1;
        private static int _categoryIdCounter = 1;
        private static int _supplierIdCounter = 1;
        private static int _customerIdCounter = 1;
        private static int _employeeIdCounter = 1;
        private static int _productIdCounter = 1;
        private static int _saleIdCounter = 1;
        private static int _saleDetailIdCounter = 1;
        private static int _expenseIdCounter = 1;
        private static int _supplierTransactionIdCounter = 1;
        private static int _technicalServiceIdCounter = 1;

        // ====================================================================
        // KATEGORİ SEED DATA
        // ====================================================================

        /// <summary>
        /// Elektronik ürün kategorilerini oluşturur
        /// </summary>
        public static List<Category> GetCategories()
        {
            // Kategoriler sabit, Bogus kullanmaya gerek yok
            var categories = new List<Category>
            {
                new Category { Id = _categoryIdCounter++, Name = "Cep Telefonları", Description = "Akıllı telefonlar ve cep telefonları. iPhone, Samsung, Xiaomi, Oppo ve diğer markalar.", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-12) },
                new Category { Id = _categoryIdCounter++, Name = "Bilgisayar & Laptop", Description = "Dizüstü ve masaüstü bilgisayarlar. Gaming, iş ve öğrenci laptopları.", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-12) },
                new Category { Id = _categoryIdCounter++, Name = "Tablet & iPad", Description = "Tablet bilgisayarlar ve iPad modelleri. Çizim ve eğitim tabletleri.", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-12) },
                new Category { Id = _categoryIdCounter++, Name = "TV & Görüntü Sistemleri", Description = "LED, OLED, QLED televizyonlar. Projeksiyon ve monitörler.", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-12) },
                new Category { Id = _categoryIdCounter++, Name = "Beyaz Eşya", Description = "Buzdolabı, çamaşır makinesi, bulaşık makinesi, fırın ve ankastre ürünler.", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-12) },
                new Category { Id = _categoryIdCounter++, Name = "Küçük Ev Aletleri", Description = "Mikser, blender, kahve makinesi, ütü, elektrikli süpürge.", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-12) },
                new Category { Id = _categoryIdCounter++, Name = "Ses Sistemleri", Description = "Hoparlör, kulaklık, soundbar, home theater sistemleri.", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-12) },
                new Category { Id = _categoryIdCounter++, Name = "Fotoğraf & Kamera", Description = "DSLR, aynasız kameralar, aksiyon kameraları, drone'lar.", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-12) },
                new Category { Id = _categoryIdCounter++, Name = "Oyun Konsolları", Description = "PlayStation, Xbox, Nintendo Switch ve oyun aksesuarları.", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-12) },
                new Category { Id = _categoryIdCounter++, Name = "Akıllı Saat & Bileklik", Description = "Smartwatch, fitness tracker, akıllı bileklikler.", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-11) },
            };

            return categories;
        }

        // ====================================================================
        // MAĞAZA SEED DATA
        // ====================================================================

        /// <summary>
        /// 55 TeknoRoma mağazasını oluşturur
        /// İstanbul(20), İzmir(13), Ankara(13), Bursa(9)
        /// </summary>
        public static List<Store> GetStores()
        {
            if (_cachedStores != null)
                return _cachedStores;

            Randomizer.Seed = new Random(RandomSeed);

            var stores = new List<Store>();

            // İstanbul mağazaları (20 adet)
            var istanbulDistricts = new[] { "Kadıköy", "Beşiktaş", "Şişli", "Üsküdar", "Maltepe", "Ataşehir", "Bakırköy", "Beylikdüzü", "Pendik", "Kartal", "Avcılar", "Esenyurt", "Sultanbeyli", "Tuzla", "Sarıyer", "Kağıthane", "Başakşehir", "Bahçelievler", "Bağcılar", "Esenler" };
            for (int i = 0; i < IstanbulStoreCount; i++)
            {
                stores.Add(CreateStore("İstanbul", istanbulDistricts[i]));
            }

            // İzmir mağazaları (13 adet)
            var izmirDistricts = new[] { "Bornova", "Karşıyaka", "Konak", "Buca", "Çiğli", "Bayraklı", "Balçova", "Narlıdere", "Gaziemir", "Karabağlar", "Menemen", "Aliağa", "Torbalı" };
            for (int i = 0; i < IzmirStoreCount; i++)
            {
                stores.Add(CreateStore("İzmir", izmirDistricts[i]));
            }

            // Ankara mağazaları (13 adet)
            var ankaraDistricts = new[] { "Çankaya", "Yenimahalle", "Keçiören", "Etimesgut", "Mamak", "Sincan", "Altındağ", "Pursaklar", "Gölbaşı", "Polatlı", "Batıkent", "Eryaman", "Ümitköy" };
            for (int i = 0; i < AnkaraStoreCount; i++)
            {
                stores.Add(CreateStore("Ankara", ankaraDistricts[i]));
            }

            // Bursa mağazaları (9 adet)
            var bursaDistricts = new[] { "Nilüfer", "Osmangazi", "Yıldırım", "Gemlik", "Mudanya", "İnegöl", "Mustafakemalpaşa", "Karacabey", "Gürsu" };
            for (int i = 0; i < BursaStoreCount; i++)
            {
                stores.Add(CreateStore("Bursa", bursaDistricts[i]));
            }

            _cachedStores = stores;
            return stores;
        }

        private static Store CreateStore(string city, string district)
        {
            var faker = new Faker("tr");

            return new Store
            {
                Id = _storeIdCounter++,
                Name = $"TEKNOROMA {district} Şubesi",
                City = city,
                District = district,
                Address = faker.Address.FullAddress(),
                Phone = faker.Phone.PhoneNumber("0216 ### ## ##"),
                Email = $"{district.ToLowerInvariant().Replace('ı', 'i').Replace('ş', 's').Replace('ç', 'c').Replace('ğ', 'g').Replace('ü', 'u').Replace('ö', 'o')}@teknoroma.com",
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-12)
            };
        }

        // ====================================================================
        // DEPARTMAN SEED DATA
        // ====================================================================

        /// <summary>
        /// Her mağaza için departmanlar oluşturur
        /// Her mağazada en az 1 departman olacak şekilde toplam 30 departman
        /// </summary>
        public static List<Department> GetDepartments()
        {
            if (_cachedDepartments != null)
                return _cachedDepartments;

            var stores = GetStores(); // Cache'den al
            var departments = new List<Department>();
            var departmentTypes = new[]
            {
                (UserRole.KasaSatis, "Satış ve Müşteri Hizmetleri", "Ürün satışı, müşteri kaydı ve fatura işlemleri"),
                (UserRole.TeknikServis, "Teknik Servis", "Cihaz tamiri, arıza takibi ve servis işlemleri"),
                (UserRole.Depo, "Depo ve Lojistik", "Stok yönetimi, mal kabul ve sevkiyat"),
                (UserRole.Muhasebe, "Muhasebe ve Finans", "Finansal işlemler, gider kayıtları ve raporlama"),
                (UserRole.SubeMuduru, "Şube Yönetimi", "Mağaza yönetimi ve koordinasyon")
            };

            int departmentIndex = 0;

            // Her mağazaya en az bir departman ekle
            foreach (var store in stores)
            {
                // Her mağazaya satış departmanı ekle (zorunlu)
                departments.Add(new Department
                {
                    Id = _departmentIdCounter++,
                    Name = departmentTypes[0].Item2,
                    Description = departmentTypes[0].Item3,
                    StoreId = store.Id,
                    DepartmentType = departmentTypes[0].Item1,
                    CreatedDate = DateTime.Now.AddMonths(-12)
                });

                departmentIndex++;

                // Kalan departmanları dağıt
                if (departmentIndex < TotalDepartmentCount)
                {
                    var deptType = departmentTypes[departmentIndex % departmentTypes.Length];
                    departments.Add(new Department
                    {
                        Id = _departmentIdCounter++,
                        Name = deptType.Item2,
                        Description = deptType.Item3,
                        StoreId = store.Id,
                        DepartmentType = deptType.Item1,
                        CreatedDate = DateTime.Now.AddMonths(-12)
                    });
                    departmentIndex++;
                }
            }

            _cachedDepartments = departments.Take(TotalDepartmentCount).ToList();
            return _cachedDepartments;
        }

        // ====================================================================
        // TEDARİKÇİ SEED DATA
        // ====================================================================

        /// <summary>
        /// Elektronik ürün tedarikçilerini oluşturur
        /// </summary>
        public static List<Supplier> GetSuppliers()
        {
            if (_cachedSuppliers != null)
                return _cachedSuppliers;

            Randomizer.Seed = new Random(RandomSeed);
            var faker = new Faker<Supplier>("tr")
                .RuleFor(s => s.Id, f => _supplierIdCounter++)
                .RuleFor(s => s.CompanyName, f => f.Company.CompanyName() + " " + f.PickRandom("A.Ş.", "Ltd. Şti.", "San. ve Tic. A.Ş."))
                .RuleFor(s => s.ContactName, f => f.Name.FullName())
                .RuleFor(s => s.Phone, f => f.Phone.PhoneNumber("0212 ### ## ##"))
                .RuleFor(s => s.Email, f => f.Internet.Email())
                .RuleFor(s => s.Address, f => f.Address.FullAddress())
                .RuleFor(s => s.City, f => f.PickRandom("İstanbul", "İzmir", "Ankara", "Bursa"))
                .RuleFor(s => s.Country, f => "Türkiye")
                .RuleFor(s => s.TaxNumber, f => f.Random.ReplaceNumbers("##########"))
                .RuleFor(s => s.IsActive, f => true)
                .RuleFor(s => s.CreatedDate, f => DateTime.Now.AddMonths(-12));

            var suppliers = faker.Generate(20);

            // İlk birkaç tedarikçiyi tanınmış markalar yapalım
            suppliers[0].CompanyName = "Apple Türkiye Distribütör A.Ş.";
            suppliers[0].ContactName = "Mehmet Yılmaz";
            suppliers[0].Email = "siparis@apple-dist.com.tr";

            suppliers[1].CompanyName = "Samsung Electronics Turkey Ltd. Şti.";
            suppliers[1].ContactName = "Ayşe Demir";
            suppliers[1].Email = "b2b@samsung.com.tr";

            suppliers[2].CompanyName = "Xiaomi Türkiye Yetkili Distribütör A.Ş.";
            suppliers[2].ContactName = "Can Öztürk";
            suppliers[2].Email = "sales@xiaomi-tr.com";

            suppliers[3].CompanyName = "LG Electronics İstanbul A.Ş.";
            suppliers[3].ContactName = "Zeynep Kaya";
            suppliers[3].Email = "b2b@lg.com.tr";

            suppliers[4].CompanyName = "Sony Türkiye Pazarlama A.Ş.";
            suppliers[4].ContactName = "Ahmet Şahin";
            suppliers[4].Email = "wholesale@sony.com.tr";

            _cachedSuppliers = suppliers;
            return suppliers;
        }

        // ====================================================================
        // MÜŞTERİ SEED DATA
        // ====================================================================

        /// <summary>
        /// Müşteri kayıtlarını oluşturur (gerçekçi Türkçe isimler)
        /// </summary>
        public static List<Customer> GetCustomers()
        {
            if (_cachedCustomers != null)
                return _cachedCustomers;

            Randomizer.Seed = new Random(RandomSeed);
            var faker = new Faker<Customer>("tr")
                .RuleFor(c => c.Id, f => _customerIdCounter++)
                .RuleFor(c => c.IdentityNumber, f => f.Random.ReplaceNumbers("###########")) // 11 haneli TC
                .RuleFor(c => c.FirstName, f => f.Name.FirstName())
                .RuleFor(c => c.LastName, f => f.Name.LastName())
                .RuleFor(c => c.BirthDate, f => f.Date.Past(50, DateTime.Now.AddYears(-18))) // 18-68 yaş arası
                .RuleFor(c => c.Gender, f => f.PickRandom<Gender?>())
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber("05## ### ## ##"))
                .RuleFor(c => c.Address, f => f.Address.FullAddress())
                .RuleFor(c => c.City, f => f.PickRandom("İstanbul", "İzmir", "Ankara", "Bursa"))
                .RuleFor(c => c.IsActive, f => true)
                .RuleFor(c => c.CreatedDate, f => f.Date.Past(2));

            _cachedCustomers = faker.Generate(500); // 500 müşteri
            return _cachedCustomers;
        }

        // ====================================================================
        // ÇALIŞAN SEED DATA
        // ====================================================================

        /// <summary>
        /// 258 çalışan oluşturur ve departmanlara dağıtır
        /// </summary>
        public static List<Employee> GetEmployees()
        {
            if (_cachedEmployees != null)
                return _cachedEmployees;

            Randomizer.Seed = new Random(RandomSeed);
            var stores = GetStores(); // Cache'den al
            var departments = GetDepartments(); // Cache'den al
            var employees = new List<Employee>();

            // Her departmana en az bir çalışan atayalım
            foreach (var department in departments)
            {
                var faker = new Faker<Employee>("tr")
                    .RuleFor(e => e.Id, f => _employeeIdCounter++)
                    .RuleFor(e => e.IdentityUserId, f => Guid.NewGuid().ToString()) // AspNetUsers Id
                    .RuleFor(e => e.IdentityNumber, f => f.Random.ReplaceNumbers("###########"))
                    .RuleFor(e => e.FirstName, f => f.Name.FirstName())
                    .RuleFor(e => e.LastName, f => f.Name.LastName())
                    .RuleFor(e => e.Email, f => f.Internet.Email())
                    .RuleFor(e => e.Phone, f => f.Phone.PhoneNumber("05## ### ## ##"))
                    .RuleFor(e => e.BirthDate, f => f.Date.Past(40, DateTime.Now.AddYears(-22))) // 22-62 yaş
                    .RuleFor(e => e.HireDate, f => f.Date.Past(5))
                    .RuleFor(e => e.Salary, f => f.Random.Decimal(17000, 45000)) // Asgari ücretin üzeri
                    .RuleFor(e => e.SalesQuota, f => department.DepartmentType == UserRole.KasaSatis ? f.Random.Decimal(50000, 150000) : (decimal?)null)
                    .RuleFor(e => e.StoreId, f => department.StoreId)
                    .RuleFor(e => e.DepartmentId, f => department.Id)
                    .RuleFor(e => e.Role, f => department.DepartmentType)
                    .RuleFor(e => e.IsActive, f => true)
                    .RuleFor(e => e.CreatedDate, f => DateTime.Now.AddMonths(-12));

                // Her departmana 8-10 çalışan
                var employeeCount = new Random(department.Id).Next(8, 11);
                employees.AddRange(faker.Generate(employeeCount));

                if (employees.Count >= TotalEmployeeCount)
                    break;
            }

            _cachedEmployees = employees.Take(TotalEmployeeCount).ToList();
            return _cachedEmployees;
        }

        // ====================================================================
        // ÜRÜN SEED DATA
        // ====================================================================

        /// <summary>
        /// Gerçekçi elektronik ürünler oluşturur
        /// </summary>
        public static List<Product> GetProducts()
        {
            if (_cachedProducts != null)
                return _cachedProducts;

            Randomizer.Seed = new Random(RandomSeed);
            var categories = GetCategories(); // Direkt çağır (zaten her seferinde aynı değerleri döndürüyor)
            var suppliers = GetSuppliers(); // Cache'den al
            var products = new List<Product>();

            // Kategori bazında ürün isimleri
            var productTemplates = new Dictionary<string, string[]>
            {
                ["Cep Telefonları"] = new[] {
                    "iPhone 15 Pro Max 256GB", "iPhone 15 128GB", "Samsung Galaxy S24 Ultra",
                    "Samsung Galaxy S24", "Xiaomi 14 Pro", "Xiaomi Redmi Note 13",
                    "Oppo Find X7", "Realme GT5", "Google Pixel 8 Pro"
                },
                ["Bilgisayar & Laptop"] = new[] {
                    "MacBook Pro 14\" M3", "MacBook Air 13\" M2", "Dell XPS 15",
                    "HP Pavilion Gaming", "Lenovo ThinkPad X1", "Asus ROG Strix G15",
                    "MSI Gaming GF63", "Acer Aspire 5"
                },
                ["Tablet & iPad"] = new[] {
                    "iPad Pro 12.9\" M2", "iPad Air 10.9\"", "Samsung Galaxy Tab S9",
                    "Xiaomi Pad 6", "Lenovo Tab P11"
                },
                ["TV & Görüntü Sistemleri"] = new[] {
                    "Samsung 65\" QLED 4K", "LG 55\" OLED", "Sony Bravia 75\" 4K",
                    "Xiaomi TV A Pro 55\"", "TCL 43\" Smart TV"
                },
                ["Beyaz Eşya"] = new[] {
                    "Samsung Buzdolabı No Frost", "LG Çamaşır Makinesi 9kg",
                    "Bosch Bulaşık Makinesi", "Arçelik Ankastre Fırın"
                },
                ["Küçük Ev Aletleri"] = new[] {
                    "Philips Airfryer XXL", "Tefal Blender", "Arçelik K3300 Kahve Makinesi",
                    "Dyson V15 Detect Süpürge", "Braun Mikser"
                },
                ["Ses Sistemleri"] = new[] {
                    "Sony WH-1000XM5 Kulaklık", "JBL Flip 6 Bluetooth Hoparlör",
                    "Bose QuietComfort 45", "Samsung HW-Q800C Soundbar"
                },
                ["Fotoğraf & Kamera"] = new[] {
                    "Canon EOS R6 Mark II", "Sony A7 IV", "Nikon Z6 II",
                    "DJI Mini 4 Pro Drone", "GoPro Hero 12"
                },
                ["Oyun Konsolları"] = new[] {
                    "PlayStation 5", "Xbox Series X", "Nintendo Switch OLED",
                    "Steam Deck"
                },
                ["Akıllı Saat & Bileklik"] = new[] {
                    "Apple Watch Series 9", "Samsung Galaxy Watch 6",
                    "Xiaomi Smart Band 8", "Huawei Watch GT 4"
                }
            };

            foreach (var category in categories)
            {
                if (!productTemplates.ContainsKey(category.Name))
                    continue;

                var templates = productTemplates[category.Name];

                foreach (var template in templates)
                {
                    var faker = new Faker<Product>("tr")
                        .RuleFor(p => p.Id, f => _productIdCounter++)
                        .RuleFor(p => p.Name, f => template)
                        .RuleFor(p => p.Description, f => f.Lorem.Sentence(10))
                        .RuleFor(p => p.Barcode, f => f.Commerce.Ean13())
                        .RuleFor(p => p.UnitPrice, f => f.Random.Decimal(500, 50000))
                        .RuleFor(p => p.UnitsInStock, f => f.Random.Number(0, 100))
                        .RuleFor(p => p.CriticalStockLevel, f => 10)
                        .RuleFor(p => p.StockStatus, (f, p) =>
                            p.UnitsInStock == 0 ? StockStatus.Tukendi :
                            p.UnitsInStock <= p.CriticalStockLevel ? StockStatus.Kritik :
                            StockStatus.Yeterli)
                        .RuleFor(p => p.CategoryId, f => category.Id)
                        .RuleFor(p => p.SupplierId, f => f.PickRandom(suppliers).Id)
                        .RuleFor(p => p.IsActive, f => true)
                        .RuleFor(p => p.ImageUrl, f => null)
                        .RuleFor(p => p.CreatedDate, f => DateTime.Now.AddMonths(-10));

                    products.Add(faker.Generate());
                }
            }

            _cachedProducts = products;
            return products;
        }

        // ====================================================================
        // SATIŞ SEED DATA
        // ====================================================================

        /// <summary>
        /// Satış kayıtlarını oluşturur (son 6 ayın satışları)
        /// </summary>
        public static List<Sale> GetSales()
        {
            Randomizer.Seed = new Random(RandomSeed);
            var customers = GetCustomers();
            var employees = GetEmployees();
            var stores = GetStores();
            var sales = new List<Sale>();

            // Son 6 ay için satışlar oluştur (yaklaşık 1000 satış)
            for (int i = 0; i < 1000; i++)
            {
                var faker = new Faker<Sale>("tr")
                    .RuleFor(s => s.Id, f => _saleIdCounter++)
                    .RuleFor(s => s.SaleNumber, (f, s) => $"S-2024-{s.Id:00000}")
                    .RuleFor(s => s.SaleDate, f => f.Date.Between(DateTime.Now.AddMonths(-6), DateTime.Now))
                    .RuleFor(s => s.CustomerId, f => f.PickRandom(customers).Id)
                    .RuleFor(s => s.EmployeeId, f => f.PickRandom(employees).Id)
                    .RuleFor(s => s.StoreId, f => f.PickRandom(stores).Id)
                    .RuleFor(s => s.PaymentType, f => f.PickRandom<PaymentType>())
                    .RuleFor(s => s.Status, f => f.PickRandom(SaleStatus.Tamamlandi, SaleStatus.Tamamlandi, SaleStatus.Tamamlandi, SaleStatus.Iptal)) // %75 tamamlandı
                    .RuleFor(s => s.Subtotal, f => 0) // SaleDetail eklenince hesaplanacak
                    .RuleFor(s => s.TaxAmount, f => 0)
                    .RuleFor(s => s.TotalAmount, f => 0)
                    .RuleFor(s => s.CreatedDate, f => f.Date.Recent(180));

                sales.Add(faker.Generate());
            }

            return sales;
        }

        /// <summary>
        /// Satış detaylarını oluşturur (her satışta 1-5 ürün)
        /// </summary>
        public static List<SaleDetail> GetSaleDetails()
        {
            Randomizer.Seed = new Random(RandomSeed);
            var sales = GetSales();
            var products = GetProducts();
            var saleDetails = new List<SaleDetail>();

            foreach (var sale in sales)
            {
                // Her satışta 1-5 arası ürün
                var itemCount = new Random(sale.Id).Next(1, 6);
                var selectedProducts = new Faker().PickRandom(products, itemCount).ToList();

                foreach (var product in selectedProducts)
                {
                    var quantity = new Random(sale.Id + product.Id).Next(1, 4); // 1-3 adet
                    var unitPrice = product.UnitPrice;
                    var lineTotal = quantity * unitPrice;

                    saleDetails.Add(new SaleDetail
                    {
                        Id = _saleDetailIdCounter++,
                        SaleId = sale.Id,
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        Subtotal = lineTotal,
                        DiscountPercentage = 0,
                        DiscountAmount = 0,
                        TotalAmount = lineTotal,
                        CreatedDate = sale.SaleDate
                    });
                }
            }

            return saleDetails;
        }

        // ====================================================================
        // GİDER SEED DATA
        // ====================================================================

        /// <summary>
        /// Gider kayıtlarını oluşturur (aylık giderler)
        /// </summary>
        public static List<Expense> GetExpenses()
        {
            Randomizer.Seed = new Random(RandomSeed);
            var stores = GetStores();
            var employees = GetEmployees();
            var expenses = new List<Expense>();

            // Son 12 ay için giderler
            for (int month = 1; month <= 12; month++)
            {
                var expenseDate = DateTime.Now.AddMonths(-month);

                // Her mağaza için aylık giderler
                foreach (var store in stores)
                {
                    // Kira gideri
                    var rentExpenseId = _expenseIdCounter++;
                    expenses.Add(new Expense
                    {
                        Id = rentExpenseId,
                        ExpenseNumber = $"G-2024-{rentExpenseId:00000}",
                        Description = $"{store.Name} - Aylık Kira",
                        ExpenseDate = expenseDate,
                        Amount = new Faker().Random.Decimal(15000, 50000),
                        Currency = Currency.TRY,
                        ExpenseType = ExpenseType.Fatura,
                        StoreId = store.Id,
                        IsPaid = true,
                        PaymentDate = expenseDate.AddDays(5),
                        CreatedDate = expenseDate
                    });

                    // Elektrik gideri
                    var electricityExpenseId = _expenseIdCounter++;
                    expenses.Add(new Expense
                    {
                        Id = electricityExpenseId,
                        ExpenseNumber = $"G-2024-{electricityExpenseId:00000}",
                        Description = $"{store.Name} - Elektrik Faturası",
                        ExpenseDate = expenseDate,
                        Amount = new Faker().Random.Decimal(3000, 8000),
                        Currency = Currency.TRY,
                        ExpenseType = ExpenseType.Fatura,
                        StoreId = store.Id,
                        IsPaid = true,
                        PaymentDate = expenseDate.AddDays(7),
                        CreatedDate = expenseDate
                    });
                }

                // Teknik altyapı giderleri (toplam için)
                var cloudExpenseId = _expenseIdCounter++;
                expenses.Add(new Expense
                {
                    Id = cloudExpenseId,
                    ExpenseNumber = $"G-2024-{cloudExpenseId:00000}",
                    Description = "Azure Cloud Services - Aylık Abonelik",
                    ExpenseDate = expenseDate,
                    Amount = 500, // USD
                    Currency = Currency.USD,
                    ExchangeRate = 32.5m,
                    ExpenseType = ExpenseType.TeknikAltyapiGideri,
                    IsPaid = true,
                    PaymentDate = expenseDate.AddDays(3),
                    CreatedDate = expenseDate
                });
            }

            return expenses.Take(500).ToList(); // 500 gider kaydı ile sınırla
        }

        // ====================================================================
        // TEDARİKÇİ İŞLEM SEED DATA
        // ====================================================================

        /// <summary>
        /// Tedarikçi işlemlerini oluşturur (ürün alımları)
        /// </summary>
        public static List<SupplierTransaction> GetSupplierTransactions()
        {
            Randomizer.Seed = new Random(RandomSeed);
            var suppliers = GetSuppliers();
            var products = GetProducts();
            var employees = GetEmployees();
            var transactions = new List<SupplierTransaction>();

            // Son 12 ay için tedarikçi alımları
            for (int i = 0; i < 200; i++)
            {
                var product = new Faker().PickRandom(products);
                var quantity = new Faker().Random.Number(10, 100);
                var unitPrice = product.UnitPrice * 0.7m; // %30 kar marjı
                var totalAmount = quantity * unitPrice;
                var transactionId = _supplierTransactionIdCounter++;

                transactions.Add(new SupplierTransaction
                {
                    Id = transactionId,
                    TransactionNumber = $"TH-2024-{transactionId:00000}",
                    TransactionDate = new Faker().Date.Between(DateTime.Now.AddMonths(-12), DateTime.Now.AddMonths(-1)),
                    SupplierId = product.SupplierId,
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalAmount = totalAmount,
                    InvoiceNumber = new Faker().Random.ReplaceNumbers("FAT-####-######"),
                    IsPaid = new Faker().Random.Bool(0.8f), // %80 ödenmiş
                    PaymentDate = new Faker().Random.Bool(0.8f) ? (DateTime?)new Faker().Date.Recent(30) : null,
                    CreatedDate = new Faker().Date.Recent(365)
                });
            }

            return transactions;
        }

        // ====================================================================
        // TEKNİK SERVİS SEED DATA
        // ====================================================================

        /// <summary>
        /// Teknik servis kayıtlarını oluşturur
        /// </summary>
        public static List<TechnicalService> GetTechnicalServices()
        {
            Randomizer.Seed = new Random(RandomSeed);
            var customers = GetCustomers();
            var products = GetProducts();
            var employees = GetEmployees();
            var stores = GetStores();
            var services = new List<TechnicalService>();

            var issueTitles = new[]
            {
                "Cihaz açılmıyor",
                "Ekran kırık",
                "Şarj olmuyor",
                "Ses gelmiyor",
                "Kamera çalışmıyor",
                "Wi-Fi bağlanmıyor",
                "Donma sorunu",
                "Batarya şişmiş"
            };

            // 100 teknik servis kaydı
            for (int i = 0; i < 100; i++)
            {
                var reportDate = new Faker().Date.Between(DateTime.Now.AddMonths(-6), DateTime.Now);
                var isResolved = new Faker().Random.Bool(0.7f); // %70 çözülmüş
                var serviceId = _technicalServiceIdCounter++;

                services.Add(new TechnicalService
                {
                    Id = serviceId,
                    ServiceNumber = $"TS-2024-{serviceId:00000}",
                    Title = new Faker().PickRandom(issueTitles),
                    Description = new Faker("tr").Lorem.Paragraph(),
                    ReportedDate = reportDate,
                    Priority = new Faker().Random.Number(1, 4),
                    Status = isResolved ? TechnicalServiceStatus.Tamamlandi : new Faker().PickRandom<TechnicalServiceStatus>(),
                    IsCustomerIssue = new Faker().Random.Bool(0.8f), // %80 müşteri sorunu
                    CustomerId = new Faker().Random.Bool(0.8f) ? new Faker().PickRandom(customers).Id : (int?)null,
                    ReportedByEmployeeId = new Faker().PickRandom(employees).Id,
                    AssignedToEmployeeId = new Faker().PickRandom(employees.Where(e => e.Role == UserRole.TeknikServis)).Id,
                    StoreId = new Faker().PickRandom(stores).Id,
                    Resolution = isResolved ? new Faker("tr").Lorem.Sentence() : null,
                    ResolvedDate = isResolved ? (DateTime?)reportDate.AddDays(new Faker().Random.Number(1, 10)) : null,
                    CreatedDate = reportDate
                });
            }

            return services;
        }
    }
}

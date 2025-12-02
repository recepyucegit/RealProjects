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

        // MAĞAZA SAYILARI
        // ---------------
        // TeknoRoma'nın Türkiye'deki mağaza dağılımı (iş gereksinimi)
        // Bu sabitler, şirketin gerçek organizasyon yapısını yansıtır
        private const int IstanbulStoreCount = 20;  // En büyük pazar
        private const int IzmirStoreCount = 13;     // 2. büyük pazar
        private const int AnkaraStoreCount = 13;    // 3. büyük pazar
        private const int BursaStoreCount = 9;      // 4. büyük pazar
        private const int TotalStoreCount = 55;     // Toplam: 20+13+13+9 = 55 mağaza

        private const int TotalDepartmentCount = 30;   // Her mağazada 30 departman
        private const int TotalEmployeeCount = 258;    // 55 mağazada toplam 258 çalışan

        // RANDOM SEED DEĞERİ
        // ------------------
        // AMACI: Her migration'da aynı verilerin üretilmesini sağlamak
        //
        // NEDEN ÖNEMLİ?
        // - Migration'lar deterministik olmalı (her seferinde aynı sonuç)
        // - Test senaryoları tutarlı olmalı
        // - Geliştirme ortamında herkes aynı verileri görmeli
        //
        // NASIL ÇALIŞIR?
        // Bogus kütüphanesine aynı seed verildiğinde, aynı rastgele değerler üretilir
        // Örnek: Seed=12345 ile "Ali Veli" üretildiyse, her zaman "Ali Veli" üretir
        private const int RandomSeed = 12345;

        // ====================================================================
        // CACHE MEKANİZMASI - Veriler bir kez oluşturulup tekrar kullanılır
        // ====================================================================
        //
        // AMACI: Performans optimizasyonu ve tutarlılık
        //
        // NEDEN CACHE KULLANIYORUZ?
        // 1. PERFORMANS: Binlerce kayıt oluşturmak CPU-intensive bir işlem
        //    - GetProducts() her çağrıldığında yeniden üretmek yerine
        //    - İlk çağrıda üret, sonraki çağrılarda cache'ten döndür
        //
        // 2. TUTARLILIK: İlişkili entity'ler aynı verileri kullanmalı
        //    - GetSales() içinde GetCustomers() çağrılıyor
        //    - GetSaleDetails() içinde de GetCustomers() çağrılıyor
        //    - Her ikisinde de aynı müşteri listesi kullanılmalı
        //
        // 3. FOREIGN KEY GÜVENLİĞİ:
        //    - SaleDetail.ProductId, Product.Id'ye referans vermeli
        //    - Product listesi farklı olursa ID'ler uyuşmaz
        //
        // NASIL ÇALIŞIR?
        // - İlk çağrı: Cache null → Veri üret → Cache'e kaydet → Döndür
        // - Sonraki çağrılar: Cache dolu → Doğrudan cache'ten döndür
        //
        // NULLABLE (?) NEDİR?
        // - List<Store>? → null olabilir demek
        // - İlk durumda null, veri üretilince dolu
        private static List<Store>? _cachedStores;
        private static List<Supplier>? _cachedSuppliers;
        private static List<Department>? _cachedDepartments;
        private static List<Customer>? _cachedCustomers;
        private static List<Employee>? _cachedEmployees;
        private static List<Product>? _cachedProducts;
        private static List<Sale>? _cachedSales;
        private static List<SaleDetail>? _cachedSaleDetails;
        private static List<Expense>? _cachedExpenses;
        private static List<SupplierTransaction>? _cachedSupplierTransactions;
        private static List<TechnicalService>? _cachedTechnicalServices;

        // ====================================================================
        // ID COUNTER'LAR (SAYAÇLAR) - Kontrollü ID Üretimi
        // ====================================================================
        //
        // AMACI: Her entity için sıralı ve öngörülebilir ID'ler üretmek
        //
        // NEDEN KULLANIYORUZ?
        // 1. FOREIGN KEY GÜVENLİĞİ:
        //    - Product.CategoryId = 1 → Category tablosunda ID=1 olmalı
        //    - Rastgele ID'lerle bu garanti edilemez
        //    - Counter ile ID=1,2,3,4... şeklinde sıralı gider
        //
        // 2. MİGRATION TUTARLILIĞI:
        //    - Aynı migration farklı zamanlarda çalıştırılabilir
        //    - Her seferinde aynı ID'ler üretilmeli
        //    - Counter + RandomSeed = Deterministik sonuç
        //
        // 3. HATA AYIKLAMA:
        //    - ID=42 olan ürün her zaman aynı ürün
        //    - Log'larda ID takibi kolay
        //    - Test senaryoları tekrarlanabilir
        //
        // KRİTİK: RESET MEKANİZMASI
        // -------------------------
        // Her Get metodu başında counter'ı 1'e reset etmeliyiz!
        //
        // ÖRNEK SORUN (Reset Olmazsa):
        // - İlk çağrı: GetCategories() → ID'ler: 1,2,3,4,5
        // - Counter değeri: 6 (son kullanılan ID + 1)
        // - İkinci çağrı: GetCategories() → ID'ler: 6,7,8,9,10 ❌ YANLIŞ!
        // - Beklenen: 1,2,3,4,5 olmalıydı
        //
        // ÇÖZÜM:
        // Her metodun başında: _categoryIdCounter = 1;
        // Böylece her çağrıda 1'den başlar
        //
        // STATIC NEDEN?
        // - Tüm metodlar arasında paylaşılır
        // - Sınıf instance'ı oluşturmadan kullanılabilir
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
        /// Elektronik ürün kategorilerini oluşturur (10 adet)
        ///
        /// NEDEN CACHE YOK?
        /// - Category sayısı az (10 adet)
        /// - Sabit veriler, her seferinde aynı
        /// - Cache'lemenin performans kazancı minimal
        ///
        /// NEDEN BOGUS YOK?
        /// - Kategori isimleri iş kuralı (Cep Telefonları, Laptop vb.)
        /// - Rastgele üretmek mantıklı değil
        /// - Manuel tanımlamak daha doğru
        /// </summary>
        public static List<Category> GetCategories()
        {
            // ID COUNTER RESET - KRİTİK!
            // Her çağrıda ID'ler 1'den başlamalı
            // Yoksa: İlk çağrı 1-10, ikinci çağrı 11-20 olur (YANLIŞ!)
            _categoryIdCounter = 1;

            // SABİT KATEGORİLER
            // İş gereksinimi: TeknoRoma'nın standart ürün kategorileri
            // Bogus kullanmıyoruz çünkü bu isimler şirket standardı
            var categories = new List<Category>
            {
                // _categoryIdCounter++ AÇIKLAMASI:
                // ++ operatörü: "Değeri kullan, sonra 1 artır" anlamına gelir
                // İlk satır: Id = 1 (sonra counter = 2 olur)
                // İkinci satır: Id = 2 (sonra counter = 3 olur)
                // Böylece ID'ler otomatik 1,2,3,4... şeklinde artar
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
        ///
        /// NEDEN CACHE VAR?
        /// - 55 mağaza birçok yerde kullanılıyor (Employee, Department, Sale, Expense)
        /// - Her çağrıda yeniden üretmek yerine cache'ten dönüyoruz
        /// - Aynı mağaza listesi = Tutarlı foreign key ilişkileri
        ///
        /// NEDEN BOGUS VAR?
        /// - Mağaza adresi, telefon, email gibi değişken bilgiler için
        /// - Manuel yazmak 55 mağaza için çok zaman alır
        /// - Gerçekçi veriler üretir (Faker.tr kütüphanesi)
        /// </summary>
        public static List<Store> GetStores()
        {
            // CACHE KONTROLÜ
            // Daha önce oluşturulduysa, tekrar üretme
            // Performans + Tutarlılık
            if (_cachedStores != null)
                return _cachedStores;

            // ID COUNTER RESET
            // Her çağrıda aynı ID'leri üretmek için
            _storeIdCounter = 1;

            // RANDOM SEED AYARLA
            // Bogus'un rastgele değerler üretmesi için seed veriyoruz
            // Aynı seed = Aynı sonuçlar (Deterministik)
            // Randomizer: Bogus kütüphanesinin global rastgele sayı üreteci
            Randomizer.Seed = new Random(RandomSeed);

            var stores = new List<Store>();

            // İSTANBUL MAĞAZALARI (20 adet)
            // Gerçek ilçe isimleri kullanılıyor (iş gereksinimi)
            // Array: Sabit ilçe listesi, değişmez
            var istanbulDistricts = new[] { "Kadıköy", "Beşiktaş", "Şişli", "Üsküdar", "Maltepe", "Ataşehir", "Bakırköy", "Beylikdüzü", "Pendik", "Kartal", "Avcılar", "Esenyurt", "Sultanbeyli", "Tuzla", "Sarıyer", "Kağıthane", "Başakşehir", "Bahçelievler", "Bağcılar", "Esenler" };

            // DÖNGÜ: Her ilçe için bir mağaza oluştur
            for (int i = 0; i < IstanbulStoreCount; i++)
            {
                // CreateStore helper metodu: Mağaza detaylarını Bogus ile doldurur
                // İl + İlçe bilgisi veriyoruz, geri kalan (adres, telefon) otomatik
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

            // CACHE'E KAYDET
            // Bir sonraki çağrıda bu listeyi döndürmek için
            _cachedStores = stores;
            return stores;
        }

        /// <summary>
        /// Tek bir mağaza oluşturur (Helper metod)
        ///
        /// HELPER METOD NEDEN?
        /// - Kod tekrarını önler (DRY prensibi: Don't Repeat Yourself)
        /// - 55 mağaza için aynı mantığı kullanıyoruz
        /// - Değişiklik tek yerden yapılır
        ///
        /// PARAMETRELER:
        /// - city: İl adı (İstanbul, İzmir, Ankara, Bursa)
        /// - district: İlçe adı (Kadıköy, Bornova vb.)
        /// </summary>
        private static Store CreateStore(string city, string district)
        {
            // FAKER OLUŞTUR
            // "tr": Türkçe locale kullan
            // Böylece Türkiye'ye uygun adres, telefon formatları üretilir
            var faker = new Faker("tr");

            return new Store
            {
                // ID: Counter ile otomatik artan (1,2,3...)
                Id = _storeIdCounter++,

                // NAME: String interpolation ($"...") ile dinamik isim
                // Örnek: "TEKNOROMA Kadıköy Şubesi"
                Name = $"TEKNOROMA {district} Şubesi",

                // ŞEHİR VE İLÇE: Parametre olarak alıyoruz
                City = city,
                District = district,

                // ADRES: Faker ile rastgele Türkçe adres
                // Örnek: "Cumhuriyet Mahallesi, Atatürk Caddesi No:42 D:5"
                Address = faker.Address.FullAddress(),

                // TELEFON: Belirlediğimiz formatta rastgele telefon
                // Format: 0216 ### ## ##
                // # karakteri rastgele rakamla değiştirilir
                // Örnek: "0216 456 78 90"
                Phone = faker.Phone.PhoneNumber("0216 ### ## ##"),

                // EMAIL: İlçe adından otomatik üret
                // SORUN: İlçe adında Türkçe karakter var (Kadıköy)
                // ÇÖZÜM: ToLowerInvariant() + Replace ile İngilizce karaktere çevir
                // Kadıköy → kadıköy → kadiköy → kadikoy@teknoroma.com
                // Replace zinciri: ı→i, ş→s, ç→c, ğ→g, ü→u, ö→o
                Email = $"{district.ToLowerInvariant().Replace('ı', 'i').Replace('ş', 's').Replace('ç', 'c').Replace('ğ', 'g').Replace('ü', 'u').Replace('ö', 'o')}@teknoroma.com",

                // AKTIFLIK: Tüm mağazalar aktif
                IsActive = true,

                // OLUŞTURMA TARİHİ: 12 ay önce açılmış gibi
                CreatedDate = DateTime.Now.AddMonths(-12)
            };
        }

        // ====================================================================
        // DEPARTMAN SEED DATA
        // ====================================================================

        /// <summary>
        /// Her mağaza için departmanlar oluşturur
        /// Her mağazada en az 1 departman olacak şekilde toplam 30 departman
        ///
        /// VERİTABANI KISITI (UNIQUE INDEX):
        /// - IX_Departments_StoreId_Name: Aynı mağazada aynı isimde iki departman olamaz
        /// - Bu kısıt migration'da otomatik oluşturuldu
        /// - Duplicate değer = Migration hatası!
        ///
        /// BU METODDA DÜZELTME YAPILDI:
        /// - Eski kod: Aynı mağazaya "Satış" departmanını 2 kez ekliyordu (HATA!)
        /// - Yeni kod: İlk departman her mağazaya, ikinci departman farklı tiplerden
        /// </summary>
        public static List<Department> GetDepartments()
        {
            // CACHE KONTROLÜ
            if (_cachedDepartments != null)
                return _cachedDepartments;

            // ID COUNTER RESET
            _departmentIdCounter = 1;

            // MAĞAZA LİSTESİ AL
            // GetStores() cache'den döner (daha önce oluşturuldu)
            // Foreign key için Store ID'lerine ihtiyacımız var
            var stores = GetStores();
            var departments = new List<Department>();

            // DEPARTMAN TİPLERİ
            // Tuple kullanılıyor: (UserRole, DepartmanAdı, Açıklama)
            // Item1 = UserRole enum değeri
            // Item2 = Departman adı (string)
            // Item3 = Açıklama (string)
            var departmentTypes = new[]
            {
                (UserRole.KasaSatis, "Satış ve Müşteri Hizmetleri", "Ürün satışı, müşteri kaydı ve fatura işlemleri"),
                (UserRole.TeknikServis, "Teknik Servis", "Cihaz tamiri, arıza takibi ve servis işlemleri"),
                (UserRole.Depo, "Depo ve Lojistik", "Stok yönetimi, mal kabul ve sevkiyat"),
                (UserRole.Muhasebe, "Muhasebe ve Finans", "Finansal işlemler, gider kayıtları ve raporlama"),
                (UserRole.SubeMuduru, "Şube Yönetimi", "Mağaza yönetimi ve koordinasyon")
            };

            // DEPARTMAN INDEX: Toplam kaç departman eklediğimizi takip eder
            int departmentIndex = 0;

            // HER MAĞAZAYA DEPARTMAN EKLE
            foreach (var store in stores)
            {
                // 1. DEPARTMAN: SATIŞ (ZORUNLU)
                // Her mağazada mutlaka bir satış departmanı olmalı (iş kuralı)
                // departmentTypes[0] = İlk eleman = Satış departmanı
                departments.Add(new Department
                {
                    Id = _departmentIdCounter++,
                    Name = departmentTypes[0].Item2,          // "Satış ve Müşteri Hizmetleri"
                    Description = departmentTypes[0].Item3,   // Açıklama
                    StoreId = store.Id,                       // Foreign Key: Hangi mağaza
                    DepartmentType = departmentTypes[0].Item1, // UserRole.KasaSatis
                    CreatedDate = DateTime.Now.AddMonths(-12)
                });

                departmentIndex++; // Sayacı artır (1 departman eklendi)

                // 2. DEPARTMAN: DİĞER TİPLERDEN (OPSIYONEL)
                // Toplam 30 departman hedefi, daha eklenecek yer varsa devam et
                if (departmentIndex < TotalDepartmentCount)
                {
                    // ⚠️ KRİTİK: DUPLICATE KEY FIX!
                    // ==========================================
                    // SORUN: (departmentIndex % departmentTypes.Length) kullanırsak
                    //        Her 5. iterasyonda index=0 olur
                    //        Index=0 → Satış departmanı
                    //        Aynı mağazaya 2. kez "Satış" eklenemez (UNIQUE INDEX hatası!)
                    //
                    // ÇÖZÜM: İlk index'i (0=Satış) atla, sadece 1-4 arası kullan
                    //
                    // FORMÜL AÇIKLAMASI:
                    // (departmentIndex % (departmentTypes.Length - 1)) + 1
                    //
                    // Örnek: departmentTypes.Length = 5 (0,1,2,3,4)
                    //        İstediğimiz: 1,2,3,4 (Satış hariç)
                    //
                    // departmentIndex=1: (1 % 4) + 1 = 1 + 1 = 2 → Teknik Servis
                    // departmentIndex=2: (2 % 4) + 1 = 2 + 1 = 3 → Depo
                    // departmentIndex=3: (3 % 4) + 1 = 3 + 1 = 4 → Muhasebe
                    // departmentIndex=4: (4 % 4) + 1 = 0 + 1 = 1 → Şube Yönetimi
                    // departmentIndex=5: (5 % 4) + 1 = 1 + 1 = 2 → Teknik Servis (döngü)
                    //
                    // ASLA 0 DÖNDÜRMEZ! ✅
                    var typeIndex = (departmentIndex % (departmentTypes.Length - 1)) + 1;
                    var deptType = departmentTypes[typeIndex];

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

            // TOPLAM 30 DEPARTMAN İLE SINIRLA
            // Döngü 55 mağaza x 2 departman = 110 departman üretebilir
            // Sadece ilk 30'unu al (iş gereksinimi)
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

            // ID counter'ı sıfırla (her çağrıda aynı ID'leri kullanmak için)
            _supplierIdCounter = 1;

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
        ///
        /// BOGUS FAKER<T> KULLANIMI:
        /// - Faker<Customer>: Customer tipinde objeler üretir
        /// - "tr": Türkçe locale (Türk isimleri, Türkiye adresleri)
        /// - RuleFor: Her property için kural tanımlar
        /// - Generate(n): n adet obje üretir
        ///
        /// AVANTAJLARI:
        /// - 500 müşteri için tek tek yazmaya gerek yok
        /// - Gerçekçi ve tutarlı veriler
        /// - Test senaryoları için ideal
        /// </summary>
        public static List<Customer> GetCustomers()
        {
            // CACHE KONTROLÜ
            if (_cachedCustomers != null)
                return _cachedCustomers;

            // ID COUNTER RESET
            _customerIdCounter = 1;

            // RANDOM SEED AYARLA
            Randomizer.Seed = new Random(RandomSeed);

            // FAKER<T> TANIMI
            // ---------------
            // Faker<Customer>: Customer tipinde nesneler üreten factory
            // "tr": Türkçe lokalizasyon (isimler, adresler Türkçe)
            //
            // RULEFOR PATTERN:
            // .RuleFor(property, factory => değer)
            // - property: Doldurulacak property (lambda: c => c.Id)
            // - factory: Faker instance'ı (genelde 'f' harfi kullanılır)
            // - değer: Üretilecek rastgele değer
            var faker = new Faker<Customer>("tr")
                // ID: Counter ile sıralı ID
                .RuleFor(c => c.Id, f => _customerIdCounter++)

                // TC KİMLİK NO: 11 haneli rastgele sayı
                // ReplaceNumbers: # karakterlerini rastgele rakamla değiştirir
                // Örnek: "###########" → "12345678901"
                .RuleFor(c => c.IdentityNumber, f => f.Random.ReplaceNumbers("###########"))

                // AD-SOYAD: Türkçe rastgele isimler
                // Name.FirstName(): Ali, Ayşe, Mehmet, Fatma vb.
                // Name.LastName(): Yılmaz, Kaya, Demir vb.
                .RuleFor(c => c.FirstName, f => f.Name.FirstName())
                .RuleFor(c => c.LastName, f => f.Name.LastName())

                // DOĞUM TARİHİ: 18-68 yaş arası
                // Past(50, refDate): refDate'ten 50 yıl öncesine kadar rastgele tarih
                // refDate = Now - 18 yıl → En genç 18 yaşında
                // 50 yıl geriye = En yaşlı 68 yaşında (18+50)
                .RuleFor(c => c.BirthDate, f => f.Date.Past(50, DateTime.Now.AddYears(-18)))

                // ⚠️ CİNSİYET: GENDER ENUM FIX!
                // ========================================
                // ESKİ KOD (HATA VERİYORDU):
                // .RuleFor(c => c.Gender, f => f.PickRandom<Gender?>())
                //
                // SORUN: PickRandom<Gender?>() boş nullable enum'dan seçmeye çalışıyor
                //        "The array is empty" hatası veriyor
                //
                // YENİ KOD (ÇÖZÜM):
                // Bool() ile rastgele true/false üret
                // true → Gender.Erkek
                // false → Gender.Kadin
                // Ternary operator: koşul ? doğruysa : yanlışsa
                .RuleFor(c => c.Gender, f => f.Random.Bool() ? Gender.Erkek : Gender.Kadin)

                // EMAIL: Rastgele email adresi
                // Örnek: "ahmet.yilmaz@gmail.com"
                .RuleFor(c => c.Email, f => f.Internet.Email())

                // TELEFON: Türkiye cep telefonu formatı
                // 05## ### ## ##
                // Örnek: "0532 456 78 90"
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber("05## ### ## ##"))

                // ADRES: Tam adres (mahalle, sokak, numara)
                .RuleFor(c => c.Address, f => f.Address.FullAddress())

                // ŞEHİR: PickRandom ile belirli şehirlerden seç
                // Sadece TeknoRoma'nın mağazası olan şehirler
                .RuleFor(c => c.City, f => f.PickRandom("İstanbul", "İzmir", "Ankara", "Bursa"))

                // AKTİFLİK: Tüm müşteriler aktif
                .RuleFor(c => c.IsActive, f => true)

                // OLUŞTURMA TARİHİ: Son 2 yıl içinde
                .RuleFor(c => c.CreatedDate, f => f.Date.Past(2));

            // GENERATE: 500 müşteri üret
            // faker.Generate(500): Yukarıdaki kurallara göre 500 adet Customer nesnesi oluşturur
            // Her çağrıda aynı 500 müşteri (RandomSeed sayesinde)
            _cachedCustomers = faker.Generate(500);
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

            // ID counter'ı sıfırla (her çağrıda aynı ID'leri kullanmak için)
            _employeeIdCounter = 1;

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
        /// Gerçekçi elektronik ürünler oluşturur (~70 ürün)
        ///
        /// ÖZEL YÖNTEM:
        /// - Ürün isimleri manuel tanımlı (gerçek ürünler: iPhone 15, Samsung S24 vb.)
        /// - Dictionary ile kategori bazlı organizasyon
        /// - Fiyat, stok vb. Bogus ile rastgele
        ///
        /// NEDEN MANUEL ÜRÜN İSİMLERİ?
        /// - Faker.Commerce.ProductName() gerçekçi elektronik ürün ismi üretemiyor
        /// - Test senaryolarında tanıdık ürünler daha anlamlı
        /// - Gerçek dünya verileriyle benzerlik önemli
        /// </summary>
        public static List<Product> GetProducts()
        {
            // CACHE KONTROLÜ
            if (_cachedProducts != null)
                return _cachedProducts;

            // ID COUNTER RESET
            _productIdCounter = 1;

            // RANDOM SEED AYARLA
            Randomizer.Seed = new Random(RandomSeed);

            // BAĞIMLI VERİLERİ AL
            // Foreign Key ilişkileri için Category ve Supplier ID'leri gerekli
            var categories = GetCategories(); // Cache yok, her seferinde aynı 10 kategoriyi döner
            var suppliers = GetSuppliers();   // Cache'den döner

            var products = new List<Product>();

            // GÜVENLİK KONTROLÜ: Boş liste kontrolü
            // Eğer kategori veya supplier yoksa ürün oluşturamayız (foreign key hatası!)
            // Migration sırasında bu durum olmamalı ama güvenlik için kontrol
            if (!categories.Any() || !suppliers.Any())
            {
                _cachedProducts = products;
                return products;
            }

            // ÜRÜN TEMPLATE'LERİ (Kategori Bazlı)
            // ====================================
            // DİCTIONARY YAPISI:
            // Key: Kategori adı (string)
            // Value: O kategorideki ürün isimleri (string array)
            //
            // NEDEN DICTIONARY?
            // - Her kategoriye özel ürünler tanımlamak için
            // - Kolay erişim: productTemplates["Cep Telefonları"]
            // - Organize ve okunabilir kod
            //
            // NEDEN ARRAY?
            // - Ürün isimleri sabit, değişmez
            // - Hafıza verimli (List'e göre)
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

            // HER KATEGORİ İÇİN ÜRÜN OLUŞTUR
            // =================================
            foreach (var category in categories)
            {
                // Bu kategorinin template'i var mı kontrol et
                // Bazı kategoriler için ürün tanımlamadıysak atla
                if (!productTemplates.ContainsKey(category.Name))
                    continue;

                // Bu kategoriye ait ürün template'lerini al
                // Örnek: "Cep Telefonları" için iPhone, Samsung vb. array'i
                var templates = productTemplates[category.Name];

                // HER ÜRÜN TEMPLATE'İ İÇİN BİR ÜRÜN OLUŞTUR
                foreach (var template in templates)
                {
                    // FAKER<PRODUCT> İLE ÜRÜN OLUŞTUR
                    var faker = new Faker<Product>("tr")
                        // ID: Sıralı counter
                        .RuleFor(p => p.Id, f => _productIdCounter++)

                        // NAME: Template'ten al (manuel tanımlı)
                        // Örnek: "iPhone 15 Pro Max 256GB"
                        .RuleFor(p => p.Name, f => template)

                        // DESCRIPTION: Lorem Ipsum (10 kelimelik açıklama)
                        // Örnek: "Yüksek çözünürlüklü ekran ile mükemmel görüntü kalitesi..."
                        .RuleFor(p => p.Description, f => f.Lorem.Sentence(10))

                        // BARCODE: EAN13 formatında barkod (13 haneli)
                        // Örnek: "5901234123457"
                        .RuleFor(p => p.Barcode, f => f.Commerce.Ean13())

                        // FİYAT: 500₺ ile 50,000₺ arası rastgele
                        .RuleFor(p => p.UnitPrice, f => f.Random.Decimal(500, 50000))

                        // STOK MİKTARI: 0-100 arası rastgele
                        // 0 = Tükendi, düşük = Kritik stok
                        .RuleFor(p => p.UnitsInStock, f => f.Random.Number(0, 100))

                        // KRİTİK STOK SEVİYESİ: Sabit 10
                        // Stok 10'un altına düşerse uyarı verilir
                        .RuleFor(p => p.CriticalStockLevel, f => 10)

                        // STOK DURUMU: Koşullu mantık!
                        // ========================================
                        // ÖZEL SYNTAX: (f, p) =>
                        // f: Faker instance (kullanmıyoruz burada)
                        // p: Oluşturulan Product nesnesi (önceki RuleFor'larla doldurulmuş)
                        //
                        // BU SAYEDE:
                        // p.UnitsInStock değerine göre p.StockStatus'u set edebiliyoruz
                        //
                        // MANTIK:
                        // Stok = 0 → Tükendi
                        // Stok ≤ 10 (CriticalLevel) → Kritik
                        // Stok > 10 → Yeterli
                        //
                        // TERNARY OPERATOR ZİNCİRİ:
                        // koşul1 ? değer1 : (koşul2 ? değer2 : değer3)
                        .RuleFor(p => p.StockStatus, (f, p) =>
                            p.UnitsInStock == 0 ? StockStatus.Tukendi :
                            p.UnitsInStock <= p.CriticalStockLevel ? StockStatus.Kritik :
                            StockStatus.Yeterli)

                        // FOREIGN KEY: CategoryId
                        // Döngüdeki category'nin ID'sini kullan
                        // Bu sayede ürün doğru kategoriye atanır
                        .RuleFor(p => p.CategoryId, f => category.Id)

                        // FOREIGN KEY: SupplierId
                        // ========================================
                        // PICKRANDOM İLE RASTGELE TEDARİKÇİ SEÇİMİ
                        //
                        // PickRandom(collection): Koleksiyondan rastgele bir eleman seçer
                        // suppliers listesinden rastgele bir Supplier seçer
                        // .Id ile seçilen Supplier'ın ID'sini alır
                        //
                        // NEDEN?
                        // Her ürünün bir tedarikçisi olmalı (iş kuralı)
                        // Hangi tedarikçiden geldiği önemli değil (rastgele dağıtalım)
                        //
                        // ÖRNEK:
                        // suppliers = [Supplier#1, Supplier#2, Supplier#3, ...]
                        // PickRandom → Supplier#7
                        // .Id → 7
                        .RuleFor(p => p.SupplierId, f => f.PickRandom(suppliers).Id)

                        // AKTİFLİK: Tüm ürünler aktif
                        .RuleFor(p => p.IsActive, f => true)

                        // GÖRSEL: Şimdilik null (ileride ürün görselleri eklenebilir)
                        .RuleFor(p => p.ImageUrl, f => null)

                        // OLUŞTURMA TARİHİ: 10 ay önce kataloga eklenmiş
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
            if (_cachedSales != null)
                return _cachedSales;

            // ID counter'ı sıfırla (her çağrıda aynı ID'leri kullanmak için)
            _saleIdCounter = 1;

            Randomizer.Seed = new Random(RandomSeed);
            var customers = GetCustomers();
            var employees = GetEmployees();
            var stores = GetStores();
            var sales = new List<Sale>();

            // Gerekli veriler yoksa boş liste döndür
            if (!customers.Any() || !employees.Any() || !stores.Any())
            {
                _cachedSales = sales;
                return sales;
            }

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

            _cachedSales = sales;
            return sales;
        }

        /// <summary>
        /// Satış detaylarını oluşturur (her satışta 1-5 ürün)
        /// </summary>
        public static List<SaleDetail> GetSaleDetails()
        {
            if (_cachedSaleDetails != null)
                return _cachedSaleDetails;

            // ID counter'ı sıfırla (her çağrıda aynı ID'leri kullanmak için)
            _saleDetailIdCounter = 1;

            Randomizer.Seed = new Random(RandomSeed);
            var sales = GetSales();
            var products = GetProducts();
            var saleDetails = new List<SaleDetail>();

            // Ürün yoksa boş liste döndür
            if (!products.Any())
            {
                _cachedSaleDetails = saleDetails;
                return saleDetails;
            }

            foreach (var sale in sales)
            {
                // Her satışta 1-5 arası ürün (ama ürün sayısından fazla seçme)
                var maxItems = Math.Min(5, products.Count);
                var itemCount = new Random(sale.Id).Next(1, maxItems + 1);
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

            _cachedSaleDetails = saleDetails;
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
            if (_cachedExpenses != null)
                return _cachedExpenses;

            // ID counter'ı sıfırla (her çağrıda aynı ID'leri kullanmak için)
            _expenseIdCounter = 1;

            Randomizer.Seed = new Random(RandomSeed);
            var stores = GetStores();
            var employees = GetEmployees();
            var expenses = new List<Expense>();

            // Store yoksa boş liste döndür
            if (!stores.Any())
            {
                _cachedExpenses = expenses;
                return expenses;
            }

            // Son 12 ay için giderler
            for (int month = 1; month <= 12; month++)
            {
                var expenseDate = DateTime.Now.AddMonths(-month);

                // Her mağaza için aylık giderler
                foreach (var store in stores)
                {
                    // Kira gideri
                    var rentExpenseId = _expenseIdCounter++;
                    var rentAmount = new Faker().Random.Decimal(15000, 50000);
                    expenses.Add(new Expense
                    {
                        Id = rentExpenseId,
                        ExpenseNumber = $"G-2024-{rentExpenseId:00000}",
                        Description = $"{store.Name} - Aylık Kira",
                        ExpenseDate = expenseDate,
                        Amount = rentAmount,
                        Currency = Currency.TRY,
                        AmountInTRY = rentAmount, // TRY ise Amount ile aynı
                        ExpenseType = ExpenseType.Fatura,
                        StoreId = store.Id,
                        IsPaid = true,
                        PaymentDate = expenseDate.AddDays(5),
                        CreatedDate = expenseDate
                    });

                    // Elektrik gideri
                    var electricityExpenseId = _expenseIdCounter++;
                    var electricityAmount = new Faker().Random.Decimal(3000, 8000);
                    expenses.Add(new Expense
                    {
                        Id = electricityExpenseId,
                        ExpenseNumber = $"G-2024-{electricityExpenseId:00000}",
                        Description = $"{store.Name} - Elektrik Faturası",
                        ExpenseDate = expenseDate,
                        Amount = electricityAmount,
                        Currency = Currency.TRY,
                        AmountInTRY = electricityAmount, // TRY ise Amount ile aynı
                        ExpenseType = ExpenseType.Fatura,
                        StoreId = store.Id,
                        IsPaid = true,
                        PaymentDate = expenseDate.AddDays(7),
                        CreatedDate = expenseDate
                    });
                }

                // Teknik altyapı giderleri (genel gider - ilk store'a ata)
                var cloudExpenseId = _expenseIdCounter++;
                var cloudAmountUSD = 500m;
                var exchangeRate = 32.5m;
                expenses.Add(new Expense
                {
                    Id = cloudExpenseId,
                    ExpenseNumber = $"G-2024-{cloudExpenseId:00000}",
                    Description = "Azure Cloud Services - Aylık Abonelik",
                    ExpenseDate = expenseDate,
                    Amount = cloudAmountUSD, // USD
                    Currency = Currency.USD,
                    ExchangeRate = exchangeRate,
                    AmountInTRY = cloudAmountUSD * exchangeRate, // USD'yi TRY'ye çevir
                    ExpenseType = ExpenseType.TeknikAltyapiGideri,
                    StoreId = stores.First().Id, // Genel gider - merkez mağazaya ata
                    IsPaid = true,
                    PaymentDate = expenseDate.AddDays(3),
                    CreatedDate = expenseDate
                });
            }

            _cachedExpenses = expenses.Take(500).ToList(); // 500 gider kaydı ile sınırla
            return _cachedExpenses;
        }

        // ====================================================================
        // TEDARİKÇİ İŞLEM SEED DATA
        // ====================================================================

        /// <summary>
        /// Tedarikçi işlemlerini oluşturur (ürün alımları)
        /// </summary>
        public static List<SupplierTransaction> GetSupplierTransactions()
        {
            if (_cachedSupplierTransactions != null)
                return _cachedSupplierTransactions;

            // ID counter'ı sıfırla (her çağrıda aynı ID'leri kullanmak için)
            _supplierTransactionIdCounter = 1;

            Randomizer.Seed = new Random(RandomSeed);
            var suppliers = GetSuppliers();
            var products = GetProducts();
            var employees = GetEmployees();
            var transactions = new List<SupplierTransaction>();

            // Ürün yoksa boş liste döndür
            if (!products.Any())
            {
                _cachedSupplierTransactions = transactions;
                return transactions;
            }

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

            _cachedSupplierTransactions = transactions;
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
            if (_cachedTechnicalServices != null)
                return _cachedTechnicalServices;

            // ID counter'ı sıfırla (her çağrıda aynı ID'leri kullanmak için)
            _technicalServiceIdCounter = 1;

            Randomizer.Seed = new Random(RandomSeed);
            var customers = GetCustomers();
            var products = GetProducts();
            var employees = GetEmployees();
            var stores = GetStores();
            var services = new List<TechnicalService>();

            // Gerekli veriler yoksa boş liste döndür
            if (!customers.Any() || !employees.Any() || !stores.Any())
            {
                _cachedTechnicalServices = services;
                return services;
            }

            // Teknik servis çalışanlarını önceden filtrele (boş array kontrolü için)
            var techServiceEmployees = employees.Where(e => e.Role == UserRole.TeknikServis).ToList();

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

                // AssignedToEmployeeId için güvenli seçim
                int? assignedEmployeeId = null;
                if (techServiceEmployees.Any())
                {
                    assignedEmployeeId = new Faker().PickRandom(techServiceEmployees).Id;
                }
                else if (employees.Any())
                {
                    // Teknik servis çalışanı yoksa herhangi bir çalışan seç
                    assignedEmployeeId = new Faker().PickRandom(employees).Id;
                }

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
                    AssignedToEmployeeId = assignedEmployeeId,
                    StoreId = new Faker().PickRandom(stores).Id,
                    Resolution = isResolved ? new Faker("tr").Lorem.Sentence() : null,
                    ResolvedDate = isResolved ? (DateTime?)reportDate.AddDays(new Faker().Random.Number(1, 10)) : null,
                    CreatedDate = reportDate
                });
            }

            _cachedTechnicalServices = services;
            return services;
        }
    }
}

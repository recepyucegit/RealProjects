// ===================================================================================
// TEKNOROMA - INFRASTRUCTURE DEPENDENCY INJECTION KAYIT SINIFI
// ===================================================================================
//
// Bu dosya Infrastructure katmanindaki tum servislerin DI container'a kaydini saglar.
// Program.cs'de tek satirda tum Infrastructure bagimliklarini ekleyebiliriz.
//
// DEPENDENCY INJECTION (DI) NEDIR?
// Bir sinifin bagimliklarini disaridan almasi prensibidir.
// - Constructor'da new() ile olusturmak yerine parametre olarak alinir
// - Gevrek baglilik (Loose Coupling) saglar
// - Unit test yazimini kolaylastirir (Mock nesneler)
// - SOLID prensiplerinden "D" - Dependency Inversion
//
// SERVICE LIFETIME TURLERI:
// 1. TRANSIENT: Her istek icin yeni instance
//    - Hafif, stateless servisler icin
//    - Ornek: Utility siniflar
//
// 2. SCOPED: Her HTTP request icin bir instance
//    - DbContext, Repository, UnitOfWork icin ideal
//    - Request boyunca ayni instance kullanilir
//    - Request bitince dispose edilir
//
// 3. SINGLETON: Uygulama boyunca tek instance
//    - HttpClient, Cache servisleri icin
//    - Dikkatli kullanilmali (thread-safety)
//
// KULLANIM (Program.cs):
// builder.Services.AddInfrastructureServices(builder.Configuration);
//
// ===================================================================================

using Application.Repositories;
using Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection
{
    /// <summary>
    /// Infrastructure Katmani Servis Kayit Sinifi
    ///
    /// EXTENSION METHOD PATTERN:
    /// IServiceCollection'a extension method ekleyerek
    /// temiz ve okunakli kayit saglariz
    ///
    /// KULLANIM:
    /// <code>
    /// // Program.cs
    /// builder.Services.AddInfrastructureServices(builder.Configuration);
    /// </code>
    /// </summary>
    public static class InfrastructureServiceRegistration
    {
        /// <summary>
        /// Infrastructure servislerini DI container'a kaydeder
        ///
        /// KAYDEDILEN SERVISLER:
        /// 1. DbContext (EF Core)
        /// 2. Generic Repository
        /// 3. Entity-specific Repository'ler
        /// 4. UnitOfWork
        /// 5. ExchangeRateService
        /// 6. HttpClient, MemoryCache
        ///
        /// CONFIGURATION:
        /// ConnectionString "appsettings.json" dosyasindan alinir
        /// </summary>
        /// <param name="services">DI container</param>
        /// <param name="configuration">Uygulama konfigurasyonu</param>
        /// <returns>Zincirleme cagri icin IServiceCollection</returns>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // =================================================================
            // 1. DATABASE CONTEXT KAYDI
            // =================================================================
            //
            // DbContext SCOPED olarak kaydedilir:
            // - Her HTTP request icin bir instance
            // - Ayni request'te tum repository'ler ayni context'i kullanir
            // - Request bitince otomatik dispose edilir
            //
            // CONNECTION STRING:
            // appsettings.json -> ConnectionStrings:DefaultConnection
            // =================================================================

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        // Migration assembly'si Infrastructure projesinde
                        sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);

                        // Baglanti dayanikliligi (Connection Resiliency)
                        // Gecici ag hatalarinda otomatik yeniden deneme
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });

                // Development ortaminda SQL sorgularini logla
                #if DEBUG
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                #endif
            });

            // =================================================================
            // 2. GENERIC REPOSITORY KAYDI
            // =================================================================
            //
            // OPEN GENERIC REGISTRATION:
            // typeof(IRepository<>) -> typeof(EfRepository<>)
            // Herhangi bir entity tipi icin otomatik cozumlenir
            //
            // ORNEK:
            // IRepository<Product> -> EfRepository<Product>
            // IRepository<Customer> -> EfRepository<Customer>
            // =================================================================

            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

            // =================================================================
            // 3. ENTITY-SPECIFIC REPOSITORY KAYITLARI
            // =================================================================
            //
            // Her entity icin ozel repository:
            // - Interface (Application) -> Implementation (Infrastructure)
            // - SCOPED: Request bazli yasam suresi
            //
            // NEDEN AYRI REPOSITORY?
            // Generic CRUD yetmez, entity'ye ozel sorgular gerekir:
            // - GetByBarcodeAsync (Product)
            // - GetByIdentityNumberAsync (Customer)
            // - GenerateSaleNumberAsync (Sale)
            // =================================================================

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<ISaleRepository, SaleRepository>();
            services.AddScoped<ISaleDetailRepository, SaleDetailRepository>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<ITechnicalServiceRepository, TechnicalServiceRepository>();
            services.AddScoped<ISupplierTransactionRepository, SupplierTransactionRepository>();

            // =================================================================
            // 4. UNIT OF WORK KAYDI
            // =================================================================
            //
            // UnitOfWork tum repository'lere tek noktadan erisim saglar
            // Transaction yonetimi icin kritik
            //
            // KULLANIM ORNEGI:
            // await _unitOfWork.BeginTransactionAsync();
            // await _unitOfWork.Products.UpdateStockAsync(...);
            // await _unitOfWork.Sales.AddAsync(...);
            // await _unitOfWork.CommitTransactionAsync();
            // =================================================================

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // =================================================================
            // 5. HTTPCLIENT VE MEMORYCACHE KAYDI
            // =================================================================
            //
            // HttpClient:
            // - ExchangeRateService icin (TCMB API cagrisi)
            // - IHttpClientFactory kullanarak socket exhaustion onlenir
            //
            // MemoryCache:
            // - Doviz kurlari cache icin (1 saat)
            // - Performans optimizasyonu
            // =================================================================

            services.AddHttpClient();
            services.AddMemoryCache();

            // =================================================================
            // 6. EXTERNAL SERVICES KAYDI
            // =================================================================
            //
            // Dis sistemlerle iletisim kuran servisler:
            // - ExchangeRateService: TCMB API
            // - Gelecekte: EmailService, SmsService, vb.
            // =================================================================

            services.AddScoped<IExchangeRateService, ExchangeRateService>();

            // =================================================================
            // ZINCIRLEME CAGRI ICIN RETURN
            // =================================================================
            //
            // Method chaining destegi:
            // services.AddInfrastructureServices(config)
            //         .AddApplicationServices()
            //         .AddPresentationServices();
            // =================================================================

            return services;
        }
    }
}

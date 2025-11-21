using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistance;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    /// <summary>
    /// Dependency Injection Extension Methods
    ///
    /// AMAÇ:
    /// - Infrastructure katmanının tüm servislerini DI Container'a ekler
    /// - DbContext, Repository'ler, UnitOfWork
    /// - Clean Architecture prensiplerine uygun
    ///
    /// NEDEN EXTENSION METHOD?
    /// - Program.cs'de tek satırda tüm servisleri ekleriz
    /// - Kod tekrarını önler
    /// - Bakımı kolay
    ///
    /// KULLANIM (Program.cs):
    /// <code>
    /// builder.Services.AddInfrastructure(builder.Configuration);
    /// </code>
    ///
    /// NEDEN SCOPED?
    /// - DbContext request bazlı olmalı (web uygulaması)
    /// - Her HTTP request'te yeni DbContext
    /// - Thread-safe değil, concurrent kullanım için Scoped
    ///
    /// DEPENDENCY HIERARCHY:
    /// DbContext → Repository'ler → UnitOfWork → Services → Controllers
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Infrastructure katmanının tüm servislerini ekler
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="configuration">IConfiguration (appsettings.json)</param>
        /// <returns>IServiceCollection (method chaining için)</returns>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ====== DATABASE CONTEXT ======

            // DbContext'i DI Container'a ekle
            // Connection String appsettings.json'dan okunur
            services.AddDbContext<TeknoromaDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    // Migration assembly: Infrastructure katmanı
                    b => b.MigrationsAssembly(typeof(TeknoromaDbContext).Assembly.FullName)
                );

                // Development ortamında SQL logları
#if DEBUG
                options.EnableSensitiveDataLogging(); // Parametre değerlerini göster
                options.EnableDetailedErrors(); // Detaylı hata mesajları
#endif
            });


            // ====== REPOSITORY'LER ======

            // Generic Repository (kullanılmaz genelde, ama olabilir)
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Basit Repository'ler
            services.AddScoped<ISimpleRepository<Category>, CategoryRepository>();
            services.AddScoped<ISimpleRepository<Supplier>, SupplierRepository>();
            services.AddScoped<ISimpleRepository<Store>, StoreRepository>();
            services.AddScoped<ISimpleRepository<Department>, DepartmentRepository>();

            // Özel Repository'ler
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<ISaleRepository, SaleRepository>();
            services.AddScoped<ISaleDetailRepository, SaleDetailRepository>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<ITechnicalServiceRepository, TechnicalServiceRepository>();
            services.AddScoped<ISupplierTransactionRepository, SupplierTransactionRepository>();


            // ====== UNIT OF WORK ======

            // UnitOfWork - Tüm repository'leri bir arada tutar
            services.AddScoped<IUnitOfWork, UnitOfWork>();


            return services; // Method chaining için
        }


        /// <summary>
        /// Identity servisleri için ek extension method (opsiyonel)
        /// Gelecekte kullanılabilir
        /// </summary>
        public static IServiceCollection AddInfrastructureIdentity(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ASP.NET Identity konfigürasyonu
            // Şu an için boş, ileride eklenecek

            /*
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password ayarları
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Lockout ayarları
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // User ayarları
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<TeknoromaDbContext>()
            .AddDefaultTokenProviders();
            */

            return services;
        }
    }
}

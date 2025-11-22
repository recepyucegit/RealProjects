// ===================================================================================
// TEKNOROMA - APPLICATION DEPENDENCY INJECTION KAYIT SINIFI
// ===================================================================================
//
// Bu dosya Application katmanindaki servislerin DI container'a kaydini saglar.
// Ileride eklenecek Application servisleri (ornegin: Validators, MediatR handlers)
// burada kaydedilecektir.
//
// APPLICATION KATMANI NEDIR?
// Onion Architecture'da is mantigi (Business Logic) katmanidir:
// - Use Case'ler (Kullanim Senaryolari)
// - Application Services (Is Servisleri)
// - DTOs (Data Transfer Objects)
// - Validators (FluentValidation)
// - MediatR Handlers (CQRS Pattern)
//
// KATMAN BAGIMLILIKLARI:
// Application -> Domain (sadece Domain'e bagimli)
// Infrastructure -> Application (ters bagimlilik yok!)
//
// KULLANIM (Program.cs):
// builder.Services.AddApplicationServices();
//
// ===================================================================================

using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjection
{
    /// <summary>
    /// Application Katmani Servis Kayit Sinifi
    ///
    /// MEVCUT DURUM:
    /// Simdilik servis implementation'lari Infrastructure'da
    /// Bu dosya gelecekteki genislemeler icin hazir
    ///
    /// GELECEK EKLENTILER:
    /// - FluentValidation validators
    /// - MediatR handlers (CQRS)
    /// - AutoMapper profiles
    /// - Application-level services
    /// </summary>
    public static class ApplicationServiceRegistration
    {
        /// <summary>
        /// Application servislerini DI container'a kaydeder
        ///
        /// GELECEKTE EKLENECEKLER:
        ///
        /// 1. FLUENT VALIDATION:
        /// <code>
        /// services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        /// </code>
        ///
        /// 2. AUTOMAPPER:
        /// <code>
        /// services.AddAutoMapper(Assembly.GetExecutingAssembly());
        /// </code>
        ///
        /// 3. MEDIATR (CQRS PATTERN):
        /// <code>
        /// services.AddMediatR(cfg =>
        ///     cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        /// </code>
        ///
        /// 4. PIPELINE BEHAVIORS:
        /// <code>
        /// services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        /// services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        /// </code>
        /// </summary>
        /// <param name="services">DI container</param>
        /// <returns>Zincirleme cagri icin IServiceCollection</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // =================================================================
            // GELECEK: FLUENT VALIDATION KAYDI
            // =================================================================
            //
            // FluentValidation ile DTO validasyonu:
            // - CreateProductDto icin ProductValidator
            // - CreateSaleDto icin SaleValidator
            //
            // KURULUM:
            // 1. NuGet: FluentValidation.DependencyInjectionExtensions
            // 2. Validators klasoru olustur
            // 3. Asagidaki satiri aktif et:
            //
            // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            // =================================================================

            // =================================================================
            // GELECEK: AUTOMAPPER KAYDI
            // =================================================================
            //
            // Entity <-> DTO donusumleri icin:
            // - Product -> ProductDto
            // - CreateProductDto -> Product
            //
            // KURULUM:
            // 1. NuGet: AutoMapper.Extensions.Microsoft.DependencyInjection
            // 2. Mapping klasoru olustur
            // 3. Profile siniflari yaz
            // 4. Asagidaki satiri aktif et:
            //
            // services.AddAutoMapper(Assembly.GetExecutingAssembly());
            // =================================================================

            // =================================================================
            // GELECEK: MEDIATR KAYDI (CQRS PATTERN)
            // =================================================================
            //
            // Command Query Responsibility Segregation:
            // - Commands: CreateProductCommand, UpdateStockCommand
            // - Queries: GetProductByIdQuery, GetAllProductsQuery
            // - Handlers: Her command/query icin handler
            //
            // AVANTAJLARI:
            // - Is mantigi handler'lara ayrilir
            // - Pipeline behaviors (validation, logging, caching)
            // - Decoupled architecture
            //
            // KURULUM:
            // 1. NuGet: MediatR
            // 2. Commands ve Queries klasorleri olustur
            // 3. Handler siniflari yaz
            // 4. Asagidaki satirlari aktif et:
            //
            // services.AddMediatR(cfg =>
            //     cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            // =================================================================

            // =================================================================
            // GELECEK: PIPELINE BEHAVIORS
            // =================================================================
            //
            // MediatR pipeline'ina eklenen cross-cutting concerns:
            //
            // 1. ValidationBehavior: Handler'dan once validasyon
            // 2. LoggingBehavior: Request/Response loglama
            // 3. CachingBehavior: Query sonuclarini cache'leme
            // 4. PerformanceBehavior: Yavas sorgulari tespit
            //
            // ORNEK:
            // Request -> Validation -> Logging -> Handler -> Logging -> Response
            //
            // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            // =================================================================

            return services;
        }
    }
}

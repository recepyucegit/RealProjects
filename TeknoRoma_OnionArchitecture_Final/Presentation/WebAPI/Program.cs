// ===================================================================================
// TEKNOROMA WEB API - PROGRAM.CS (Uygulama Giris Noktasi)
// ===================================================================================
//
// Bu dosya ASP.NET Core uygulamasinin giris noktasidir.
// Tum servislerin DI container'a kaydedildigi ve middleware'lerin
// yapilandirildiği yerdir.
//
// MINIMAL API vs CONTROLLER-BASED API:
// Bu projede Controller-based API kullaniyoruz cunku:
// - Daha organize ve olceklenebilir
// - Attribute-based routing
// - Action filters destegi
// - Buyuk projeler icin daha uygun
//
// TEKNOROMA GEREKSINIMLERI:
// - Rol bazli yetkilendirme (JWT)
// - Swagger dokumantasyonu
// - CORS politikasi (Web client icin)
// - Global exception handling
// - Request/Response logging
//
// ===================================================================================

using Application.DependencyInjection;
using Infrastructure.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using WebAPI.Extensions;
using WebAPI.Middlewares;

// ===================================================================================
// SERILOG YAPILANDIRMASI
// ===================================================================================
// Uygulama baslamadan once log altyapisini kur
// Bootstrap logger: Uygulama ayaga kalkarken olusabilecek hatalari yakalar

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/teknoroma-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("TeknoRoma Web API baslatiliyor...");

    var builder = WebApplication.CreateBuilder(args);

    // ===================================================================================
    // SERILOG ENTEGRASYONU
    // ===================================================================================
    // ASP.NET Core'un default logger'ini Serilog ile degistir

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/teknoroma-.log", rollingInterval: RollingInterval.Day));

    // ===================================================================================
    // DEPENDENCY INJECTION - SERVIS KAYITLARI
    // ===================================================================================

    // Infrastructure servisleri (DbContext, Repositories, UnitOfWork)
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Application servisleri (Validators, AutoMapper - gelecekte)
    builder.Services.AddApplicationServices();

    // ===================================================================================
    // CONTROLLER YAPILANDIRMASI
    // ===================================================================================

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // JSON serialization ayarlari
            // Enum'lari string olarak serialize et (okunabilirlik)
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());

            // Null degerleri JSON'a dahil etme
            options.JsonSerializerOptions.DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

    // ===================================================================================
    // SWAGGER / OPENAPI YAPILANDIRMASI
    // ===================================================================================
    // API dokumantasyonu ve test arayuzu
    // Gelistirme ortaminda: https://localhost:xxxx/swagger

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "TeknoRoma API",
            Version = "v1",
            Description = @"
TeknoRoma Elektronik Market Yonetim Sistemi API

## Ozellikler
- Urun ve Stok Yonetimi
- Satis Islemleri (POS)
- Musteri Yonetimi
- Tedarikci Yonetimi
- Calisdan Yonetimi
- Teknik Servis Talepleri
- Gider Takibi
- Raporlama
- Doviz Kuru Entegrasyonu

## Kullanici Rolleri
- **SubeMuduru**: Tam yetki
- **KasaSatis**: Satis islemleri
- **MobilSatis**: Mobil satis
- **Depo**: Stok islemleri
- **Muhasebe**: Finansal islemler
- **TeknikServis**: Teknik destek
",
            Contact = new OpenApiContact
            {
                Name = "TeknoRoma IT",
                Email = "it@teknoroma.com"
            }
        });

        // JWT Authentication icin Swagger ayari
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT token giriniz. Ornek: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // ===================================================================================
    // CORS YAPILANDIRMASI
    // ===================================================================================
    // Cross-Origin Resource Sharing
    // Web client'in API'ye erisebilmesi icin gerekli

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });

        // Production icin daha kisitlayici policy
        options.AddPolicy("Production", policy =>
        {
            policy.WithOrigins(
                    "https://teknoroma.com",
                    "https://www.teknoroma.com",
                    "https://admin.teknoroma.com")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    // ===================================================================================
    // JWT AUTHENTICATION YAPILANDIRMASI
    // ===================================================================================
    // Token tabanli kimlik dogrulama

    builder.Services.AddJwtAuthentication(builder.Configuration);

    // ===================================================================================
    // UYGULAMA YAPILANDIRMASI (PIPELINE)
    // ===================================================================================

    var app = builder.Build();

    // Serilog request logging
    app.UseSerilogRequestLogging();

    // Development ortaminda Swagger aktif
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "TeknoRoma API v1");
            options.RoutePrefix = "swagger";
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });
    }

    // Global Exception Handler Middleware
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // HTTPS yonlendirmesi
    app.UseHttpsRedirection();

    // CORS
    app.UseCors("AllowAll");

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Controller routing
    app.MapControllers();

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Application = "TeknoRoma API",
        Version = "1.0.0"
    })).WithTags("Health");

    Log.Information("TeknoRoma Web API basariyla baslatildi");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama baslatilirken hata olustu");
}
finally
{
    Log.CloseAndFlush();
}

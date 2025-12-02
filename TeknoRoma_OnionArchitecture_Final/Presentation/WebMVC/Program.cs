// ===================================================================================
// TEKNOROMA MVC WEB UYGULAMASI - PROGRAM.CS
// ===================================================================================
//
// Merkez sube ve yonetim paneli icin ASP.NET Core MVC uygulamasi.
//
// TEKNOROMA GEREKSINIMLERI:
// - Rol bazli yetkilendirme
// - Satis ekrani (POS)
// - Stok yonetimi
// - Raporlama (Excel, PDF ciktisi)
// - Dashboard'lar
//
// ===================================================================================

using Application.DependencyInjection;
using Infrastructure.DependencyInjection;
using Serilog;

// Serilog yapilandirmasi
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/teknoroma-mvc-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("TeknoRoma MVC Web Uygulamasi baslatiliyor...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog entegrasyonu
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/teknoroma-mvc-.log", rollingInterval: RollingInterval.Day));

    // ===================================================================================
    // DEPENDENCY INJECTION
    // ===================================================================================

    // Infrastructure servisleri (DbContext, Repositories)
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Application servisleri
    builder.Services.AddApplicationServices();

    // ===================================================================================
    // MVC YAPILANDIRMASI
    // ===================================================================================

    builder.Services.AddControllersWithViews()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // Session destegi (kullanici bilgileri icin)
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    // ===================================================================================
    // UYGULAMA YAPILANDIRMASI
    // ===================================================================================

    var app = builder.Build();

    // Serilog request logging
    app.UseSerilogRequestLogging();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();

    // Default route - Kullanıcılar ilk olarak Login sayfasını görsün
    // DEĞIŞIKLIK: Home/Index yerine Account/Login
    // SEBEP: Kimlik doğrulama gerektiren bir uygulama, giriş sayfası ile başlamalı
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");

    // Area route (Admin, Reports gibi)
    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    Log.Information("TeknoRoma MVC Web Uygulamasi basariyla baslatildi");
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

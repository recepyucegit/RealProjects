using Infrastructure;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Web
{
    /// <summary>
    /// TeknoRoma MVC Application - Entry Point
    ///
    /// ROLLER:
    /// - SubeYoneticisi: Şube Müdürü (Haluk Bey)
    /// - KasaSatis: Kasa Satış Temsilcisi (Gül Satar)
    /// - MobilSatis: Mobil Satış Temsilcisi (Fahri Cepçi)
    /// - Depo: Depo Temsilcisi (Kerim Zulacı)
    /// - Muhasebe: Muhasebe Temsilcisi (Feyza Paragöz)
    /// - TeknikServis: Teknik Servis Temsilcisi (Özgün Kablocu)
    ///
    /// AREAS:
    /// Her rol için ayrı Area (Dashboard, Controllers, Views)
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // ====== SERILOG CONFIGURATION ======
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("TeknoRoma Application Starting...");

                var builder = WebApplication.CreateBuilder(args);

                // Serilog'u kullan
                builder.Host.UseSerilog();


                // ====== SERVICES CONFIGURATION ======

                // Infrastructure katmanı (DbContext, Repositories, UnitOfWork)
                builder.Services.AddInfrastructure(builder.Configuration);

                // ASP.NET Identity
                builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    // Password ayarları
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;

                    // Lockout ayarları
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 5;

                    // User ayarları
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<TeknoromaDbContext>()
                .AddDefaultTokenProviders();

                // Cookie ayarları
                builder.Services.ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8); // 8 saatlik oturum
                    options.SlidingExpiration = true;
                });

                // AutoMapper
                builder.Services.AddAutoMapper(typeof(Program).Assembly);

                // MVC Controllers ve Views
                builder.Services.AddControllersWithViews()
                    .AddRazorRuntimeCompilation(); // Development'ta view'lar runtime'da compile edilir

                // Session (Alışveriş sepeti için)
                builder.Services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

                // HTTP Context Accessor (Session için)
                builder.Services.AddHttpContextAccessor();


                // ====== APPLICATION CONFIGURATION ======

                var app = builder.Build();

                // Development/Production ortam ayarları
                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles(); // wwwroot klasörü

                app.UseRouting();

                // Authentication & Authorization
                app.UseAuthentication();
                app.UseAuthorization();

                // Session
                app.UseSession();


                // ====== ROUTING CONFIGURATION ======

                // Area routing (Öncelikli)
                app.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

                // Default routing
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");


                // ====== SEED DATA ======
                // İlk çalıştırmada varsayılan kullanıcılar ve roller oluştur
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        await SeedData.InitializeAsync(services);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "An error occurred while seeding data");
                    }
                }


                Log.Information("TeknoRoma Application Started Successfully");
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

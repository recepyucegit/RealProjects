// ===================================================================================
// TEKNOROMA - JWT AUTHENTICATION EXTENSION
// ===================================================================================
//
// Bu dosya JWT (JSON Web Token) tabanli kimlik dogrulama yapilandirmasini icerir.
//
// JWT NEDIR?
// JSON Web Token, kullanici kimligini dogrulamak icin kullanilan
// acik standart bir token formatidir (RFC 7519).
//
// JWT YAPISI:
// xxxxx.yyyyy.zzzzz
// |     |     |
// |     |     +-- Signature (Imza)
// |     +-------- Payload (Veri - Claims)
// +-------------- Header (Algoritma bilgisi)
//
// TEKNOROMA ROLLERI:
// - SubeMuduru: Tam yetki (tum islemler)
// - KasaSatis: Satis islemleri, musteri goruntuleme
// - MobilSatis: Mobil satis, stok sorgulama
// - Depo: Stok islemleri, satis goruntuleme
// - Muhasebe: Finansal islemler, gider yonetimi
// - TeknikServis: Teknik destek talepleri
//
// ===================================================================================

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebAPI.Extensions
{
    /// <summary>
    /// JWT Authentication Extension Methods
    ///
    /// Program.cs'de kullanim:
    /// builder.Services.AddJwtAuthentication(builder.Configuration);
    /// </summary>
    public static class JwtAuthenticationExtension
    {
        /// <summary>
        /// JWT Authentication servislerini DI container'a kaydeder
        ///
        /// YAPILANDIRMA (appsettings.json):
        /// "JwtSettings": {
        ///     "SecretKey": "...",
        ///     "Issuer": "TeknoRoma",
        ///     "Audience": "TeknoRomaUsers",
        ///     "ExpirationInMinutes": 60
        /// }
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // =================================================================
            // JWT AYARLARINI CONFIGURATION'DAN AL
            // =================================================================

            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey yapilandirilmamis!");

            var issuer = jwtSettings["Issuer"] ?? "TeknoRoma";
            var audience = jwtSettings["Audience"] ?? "TeknoRomaUsers";

            // =================================================================
            // AUTHENTICATION SCHEMASI YAPILANDIRMASI
            // =================================================================

            services.AddAuthentication(options =>
            {
                // Default authentication scheme: JWT Bearer
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // =============================================================
                // TOKEN DOGRULAMA PARAMETRELERI
                // =============================================================

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Issuer (token'i olusturan) dogrulamasi
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    // Audience (token'in hedef kitlesi) dogrulamasi
                    ValidateAudience = true,
                    ValidAudience = audience,

                    // Token imza dogrulamasi
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),

                    // Token suresi dogrulamasi
                    ValidateLifetime = true,

                    // Saat farki toleransi (sunucu saatleri arasi)
                    ClockSkew = TimeSpan.Zero
                };

                // =============================================================
                // JWT OLAYLARI (Events)
                // =============================================================

                options.Events = new JwtBearerEvents
                {
                    // Token dogrulama basarisiz oldugunda
                    OnAuthenticationFailed = context =>
                    {
                        // Token suresi dolduysa header'a bilgi ekle
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },

                    // Token dogrulama basarili oldugunda
                    OnTokenValidated = context =>
                    {
                        // Burada ek kontroller yapilabilir
                        // Ornegin: Kullanici hala aktif mi?
                        return Task.CompletedTask;
                    },

                    // Yetkilendirme basarisiz (401 Unauthorized)
                    OnChallenge = context =>
                    {
                        // Custom response dondurmek icin
                        return Task.CompletedTask;
                    }
                };
            });

            // =================================================================
            // AUTHORIZATION POLITIKALARI
            // =================================================================
            //
            // Rol bazli yetkilendirme politikalari
            // Controller'larda: [Authorize(Policy = "SubeYonetimi")]

            services.AddAuthorizationBuilder()
                // Sube Yonetimi - Sadece Sube Muduru
                .AddPolicy("SubeYonetimi", policy =>
                    policy.RequireRole("SubeMuduru"))

                // Satis Islemleri - Kasa ve Mobil Satis
                .AddPolicy("SatisYapabilir", policy =>
                    policy.RequireRole("SubeMuduru", "KasaSatis", "MobilSatis"))

                // Stok Islemleri - Depo ve Sube Muduru
                .AddPolicy("StokYonetimi", policy =>
                    policy.RequireRole("SubeMuduru", "Depo"))

                // Finansal Islemler - Muhasebe ve Sube Muduru
                .AddPolicy("FinansYonetimi", policy =>
                    policy.RequireRole("SubeMuduru", "Muhasebe"))

                // Teknik Destek - Teknik Servis ve Sube Muduru
                .AddPolicy("TeknikDestek", policy =>
                    policy.RequireRole("SubeMuduru", "TeknikServis"))

                // Raporlama - Tum yoneticiler
                .AddPolicy("RaporGorebilir", policy =>
                    policy.RequireRole("SubeMuduru", "Muhasebe", "Depo"));

            return services;
        }
    }
}

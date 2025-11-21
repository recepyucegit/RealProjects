using Microsoft.EntityFrameworkCore;
using TeknoRoma.Business.Abstract;
using TeknoRoma.Business.Concrete;
using TeknoRoma.Business.Mappings;
using TeknoRoma.DataAccess.Abstract;
using TeknoRoma.DataAccess.Concrete;
using TeknoRoma.DataAccess.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. DbContext Konfigürasyonu
builder.Services.AddDbContext<TeknoRomaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Repository Pattern - Dependency Injection
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// 3. Unit of Work Pattern - Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 4. Service Layer - Dependency Injection
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// 5. AutoMapper Konfigürasyonu
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// 6. MVC Controllers ve Views
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation(); // Razor view'ları runtime'da derler (development için)

// 7. Session (Sepet için kullanılacak)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 8. HttpContextAccessor (Session erişimi için)
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot klasöründeki static dosyalara erişim

app.UseRouting();

app.UseSession(); // Session middleware

app.UseAuthentication();
app.UseAuthorization();

// MVC Route Pattern
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

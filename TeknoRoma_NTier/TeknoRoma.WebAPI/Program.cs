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
// SQL Server bağlantısı appsettings.json'dan alınır
builder.Services.AddDbContext<TeknoRomaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Repository Pattern - Dependency Injection
// IRepository<T> istendiğinde Repository<T> instance'ı verilir (Scoped lifecycle)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// 3. Unit of Work Pattern - Dependency Injection
// IUnitOfWork istendiğinde UnitOfWork instance'ı verilir
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 4. Service Layer - Dependency Injection
// IProductService istendiğinde ProductService instance'ı verilir
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
// Diğer service'ler buraya eklenecek...

// 5. AutoMapper Konfigürasyonu
// AutoMapperProfile'daki mapping kurallarını kullanır
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// 6. Controllers
builder.Services.AddControllers();

// 7. Swagger/OpenAPI Configuration
// API dokümantasyonu için Swagger kullanılır
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TeknoRoma API",
        Version = "v1",
        Description = "TeknoRoma E-Ticaret Platformu RESTful API - N-tier Architecture",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "TeknoRoma",
            Email = "info@teknoroma.com"
        }
    });
});

// 8. CORS Policy (Cross-Origin Resource Sharing)
// Frontend uygulamalarının API'yi çağırabilmesi için
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Development ortamında Swagger UI aktif
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeknoRoma API v1");
        c.RoutePrefix = string.Empty; // Swagger UI root'ta açılır (https://localhost:5001/)
    });
}

// HTTPS Redirection - HTTP isteklerini HTTPS'e yönlendirir
app.UseHttpsRedirection();

// CORS Middleware
app.UseCors("AllowAll");

// Authentication & Authorization (ileride eklenecek)
app.UseAuthentication();
app.UseAuthorization();

// Controller'ları map et
app.MapControllers();

// Uygulamayı başlat
app.Run();

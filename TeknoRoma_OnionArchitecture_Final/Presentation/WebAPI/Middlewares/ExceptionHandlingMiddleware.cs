// ===================================================================================
// TEKNOROMA - GLOBAL EXCEPTION HANDLING MIDDLEWARE
// ===================================================================================
//
// Bu middleware tum HTTP request'lerinde olusan hatalari yakalar ve
// tutarli bir hata response'u dondurur.
//
// MIDDLEWARE NEDIR?
// HTTP request pipeline'inda her request'in gectiği ara katmandir.
// Request -> Middleware1 -> Middleware2 -> ... -> Controller -> Response
//
// NEDEN GLOBAL EXCEPTION HANDLER?
// - Tum hatalari tek noktadan yonetmek
// - Tutarli hata formati (API consumer'lar icin)
// - Hassas hata bilgilerini gizlemek (Production)
// - Merkezi loglama
//
// HATA TIPLERI:
// - ValidationException: 400 Bad Request
// - UnauthorizedAccessException: 401 Unauthorized
// - KeyNotFoundException: 404 Not Found
// - Exception (genel): 500 Internal Server Error
//
// ===================================================================================

using System.Net;
using System.Text.Json;

namespace WebAPI.Middlewares
{
    /// <summary>
    /// Global Exception Handling Middleware
    ///
    /// Tum unhandled exception'lari yakalar ve
    /// standart bir hata response'u dondurur
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Middleware invoke metodu
        ///
        /// Her HTTP request bu metoddan gecer
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Sonraki middleware'e veya controller'a gecis
                await _next(context);
            }
            catch (Exception ex)
            {
                // Hata yakalandi, isle
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Exception'i isle ve uygun response dondur
        /// </summary>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // =================================================================
            // HATA TIPLERINE GORE STATUS CODE BELIRLEME
            // =================================================================

            var (statusCode, errorType, message) = exception switch
            {
                // Null parametre hatalari (ArgumentException'dan once olmali)
                ArgumentNullException nullEx =>
                    (HttpStatusCode.BadRequest, "ValidationError", nullEx.Message),

                // Validation hatalari
                ArgumentException argEx =>
                    (HttpStatusCode.BadRequest, "ValidationError", argEx.Message),

                // Yetkilendirme hatalari
                UnauthorizedAccessException =>
                    (HttpStatusCode.Unauthorized, "AuthorizationError", "Bu islemi yapmaya yetkiniz yok."),

                // Kayit bulunamadi
                KeyNotFoundException =>
                    (HttpStatusCode.NotFound, "NotFound", "Istenen kayit bulunamadi."),

                // Is kurali ihlali
                InvalidOperationException opEx =>
                    (HttpStatusCode.Conflict, "BusinessRuleViolation", opEx.Message),

                // Diger tum hatalar
                _ => (HttpStatusCode.InternalServerError, "ServerError", "Beklenmeyen bir hata olustu.")
            };

            // =================================================================
            // LOGLAMA
            // =================================================================

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                // 500 hatalari ERROR seviyesinde logla (kritik)
                _logger.LogError(exception,
                    "Sunucu hatasi: {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
            }
            else
            {
                // Diger hatalar WARNING seviyesinde
                _logger.LogWarning(
                    "HTTP {StatusCode} hatasi: {Method} {Path} - {Message}",
                    (int)statusCode,
                    context.Request.Method,
                    context.Request.Path,
                    message);
            }

            // =================================================================
            // ERROR RESPONSE OLUSTURMA
            // =================================================================

            var errorResponse = new ErrorResponse
            {
                Type = errorType,
                Message = message,
                StatusCode = (int)statusCode,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                TraceId = context.TraceIdentifier
            };

            // Development ortaminda stack trace ekle
            if (_environment.IsDevelopment())
            {
                errorResponse.Detail = exception.ToString();
            }

            // =================================================================
            // RESPONSE YAZMA
            // =================================================================

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
            await context.Response.WriteAsync(json);
        }
    }

    /// <summary>
    /// Standart Hata Response Modeli
    ///
    /// Tum API hatalarinda bu format kullanilir
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Hata tipi (ValidationError, NotFound, ServerError, vb.)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Kullaniciya gosterilecek hata mesaji
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// HTTP status kodu
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Hata zamani (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Hata olusan endpoint
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Izleme ID'si (destek talebi icin)
        /// </summary>
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Detayli hata bilgisi (sadece Development)
        /// </summary>
        public string? Detail { get; set; }
    }
}

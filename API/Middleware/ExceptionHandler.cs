using Microsoft.AspNetCore.Mvc;

namespace API.Middleware
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (statusCode, message) = ex switch
            {
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "No autorizado"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = message
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}

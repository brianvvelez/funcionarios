using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using GestionInventarios.Api.Exceptions;

namespace GestionInventarios.Api.Middleware
{
    /// <summary>
    /// Middleware global de manejo de excepciones. Captura cualquier
    /// excepción no controlada y la traduce a una respuesta
    /// ProblemDetails (RFC 7807) con el status code adecuado. De esta
    /// forma los controllers no necesitan try/catch repetitivos.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next   = next;
            _logger = logger;
            _env    = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await ManejarExcepcionAsync(context, ex);
            }
        }

        private async Task ManejarExcepcionAsync(HttpContext context, Exception ex)
        {
            var (status, title) = ex switch
            {
                AuthenticationException => (HttpStatusCode.Unauthorized, "Credenciales inválidas."),
                NotFoundException       => (HttpStatusCode.NotFound,     "Recurso no encontrado."),
                ArgumentException       => (HttpStatusCode.BadRequest,   "Solicitud inválida."),
                DAOException            => (HttpStatusCode.InternalServerError,
                                            "Error en el acceso a datos."),
                _                       => (HttpStatusCode.InternalServerError,
                                            "Ocurrió un error inesperado en el servidor.")
            };

            // Las excepciones de autenticación se registran con nivel
            // Information; el resto como Error para diferenciar la
            // operación normal del fallo real.
            if (ex is AuthenticationException)
                _logger.LogInformation(ex, "Fallo de autenticación: {Mensaje}", ex.Message);
            else
                _logger.LogError(ex, "Excepción no controlada: {Mensaje}", ex.Message);

            var problema = new ProblemDetails
            {
                Status = (int)status,
                Title  = title,
                Detail = ex.Message,
                Type   = $"https://httpstatuses.io/{(int)status}",
                Instance = context.Request.Path
            };

            // En desarrollo se devuelve la traza completa para facilitar
            // el debug; en producción se omite.
            if (_env.IsDevelopment() && ex is not AuthenticationException)
            {
                problema.Extensions["stackTrace"] = ex.ToString();
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/problem+json";

            var json = JsonSerializer.Serialize(problema,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(json);
        }
    }
}

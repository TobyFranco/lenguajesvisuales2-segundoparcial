using System.Text;
using ClienteAPI.Models;
using ClienteAPI.Services;

namespace ClienteAPI.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ILogService logService)
        {
            var log = new LogApi
            {
                DateTime = DateTime.Now,
                UrlEndpoint = context.Request.Path,
                MetodoHttp = context.Request.Method,
                DireccionIp = context.Connection.RemoteIpAddress?.ToString()
            };

            context.Request.EnableBuffering();
            var requestBody = await LeerRequestBodyAsync(context.Request);
            log.RequestBody = requestBody;
            context.Request.Body.Position = 0;

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                log.StatusCode = context.Response.StatusCode;
                log.TipoLog = context.Response.StatusCode >= 400 ? "Error" : "Info";
                
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                
                log.ResponseBody = TruncarTexto(responseText, 5000);
                log.Detalle = context.Response.StatusCode >= 400 
                    ? $"Request falló con código {context.Response.StatusCode}" 
                    : "Request exitoso";

                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                log.TipoLog = "Error";
                log.StatusCode = 500;
                log.Detalle = $"Excepción: {ex.Message}";
                log.ResponseBody = ex.ToString();

                _logger.LogError(ex, "Error no controlado en la API");

                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Error interno del servidor");
            }
            finally
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await logService.RegistrarLogAsync(log);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al guardar log en base de datos");
                    }
                });
            }
        }

        private async Task<string> LeerRequestBodyAsync(HttpRequest request)
        {
            try
            {
                if (request.ContentLength == null || request.ContentLength == 0)
                {
                    return string.Empty;
                }

                if (request.ContentType?.Contains("multipart/form-data") == true)
                {
                    return "[Multipart Form Data - Archivos]";
                }

                using var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true);

                var body = await reader.ReadToEndAsync();
                return TruncarTexto(body, 5000);
            }
            catch
            {
                return "[Error al leer request body]";
            }
        }

        private string TruncarTexto(string texto, int longitudMaxima)
        {
            if (string.IsNullOrEmpty(texto) || texto.Length <= longitudMaxima)
            {
                return texto;
            }

            return texto.Substring(0, longitudMaxima) + "... [truncado]";
        }
    }
}
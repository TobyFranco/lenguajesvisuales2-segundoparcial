using Microsoft.AspNetCore.Mvc;
using ClienteAPI.Services;

namespace ClienteAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArchivoController : ControllerBase
    {
        private readonly IArchivoService _archivoService;
        private readonly ILogger<ArchivoController> _logger;

        public ArchivoController(IArchivoService archivoService, ILogger<ArchivoController> logger)
        {
            _archivoService = archivoService;
            _logger = logger;
        }

        /// <summary>
        /// Carga múltiples archivos desde un archivo ZIP
        /// </summary>
        [HttpPost("cargar-multiples/{ci}")]
        public async Task<IActionResult> CargarArchivosMultiples(string ci, IFormFile archivoZip)
        {
            try
            {
                if (archivoZip == null || archivoZip.Length == 0)
                {
                    return BadRequest(new { mensaje = "Debe proporcionar un archivo ZIP" });
                }

                if (!archivoZip.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { mensaje = "El archivo debe ser un ZIP" });
                }

                var resultado = await _archivoService.CargarArchivosDesdeZipAsync(ci, archivoZip);

                if (!resultado.Exito)
                {
                    return BadRequest(new { mensaje = resultado.Mensaje });
                }

                return Ok(new 
                { 
                    mensaje = "Archivos cargados exitosamente",
                    cantidadArchivos = resultado.Data?.Count ?? 0,
                    archivos = resultado.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar archivos");
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todos los archivos de un cliente
        /// </summary>
        [HttpGet("cliente/{ci}")]
        public async Task<IActionResult> ObtenerArchivosPorCliente(string ci)
        {
            try
            {
                var archivos = await _archivoService.ObtenerArchivosPorClienteAsync(ci);
                return Ok(archivos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener archivos");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Descarga un archivo específico
        /// </summary>
        [HttpGet("descargar/{idArchivo}")]
        public async Task<IActionResult> DescargarArchivo(int idArchivo)
        {
            try
            {
                var resultado = await _archivoService.ObtenerArchivoFisicoAsync(idArchivo);

                if (resultado.stream == null || string.IsNullOrEmpty(resultado.nombreArchivo))
                {
                    return NotFound(new { mensaje = "Archivo no encontrado" });
                }

                return File(resultado.stream, resultado.contentType, resultado.nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar archivo");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina un archivo
        /// </summary>
        [HttpDelete("{idArchivo}")]
        public async Task<IActionResult> EliminarArchivo(int idArchivo)
        {
            try
            {
                var resultado = await _archivoService.EliminarArchivoAsync(idArchivo);

                if (!resultado)
                {
                    return NotFound(new { mensaje = "Archivo no encontrado" });
                }

                return Ok(new { mensaje = "Archivo eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar archivo");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}
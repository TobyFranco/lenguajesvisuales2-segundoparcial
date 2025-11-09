using Microsoft.AspNetCore.Mvc;
using ClienteAPI.Services;

namespace ClienteAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly ILogService _logService;
        private readonly ILogger<LogController> _logger;

        public LogController(ILogService logService, ILogger<LogController> logger)
        {
            _logService = logService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los logs registrados
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerLogs(
            [FromQuery] int pagina = 1, 
            [FromQuery] int registrosPorPagina = 50,
            [FromQuery] string? tipoLog = null)
        {
            try
            {
                var logs = await _logService.ObtenerLogsAsync(pagina, registrosPorPagina, tipoLog);
                
                return Ok(new
                {
                    pagina,
                    registrosPorPagina,
                    total = logs.Count,
                    datos = logs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene un log específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerLogPorId(int id)
        {
            try
            {
                var log = await _logService.ObtenerLogPorIdAsync(id);

                if (log == null)
                {
                    return NotFound(new { mensaje = $"Log con ID {id} no encontrado" });
                }

                return Ok(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener log");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene logs por rango de fechas
        /// </summary>
        [HttpGet("rango-fechas")]
        public async Task<IActionResult> ObtenerLogsPorFecha(
            [FromQuery] DateTime? fechaInicio, 
            [FromQuery] DateTime? fechaFin)
        {
            try
            {
                if (!fechaInicio.HasValue || !fechaFin.HasValue)
                {
                    return BadRequest(new { mensaje = "Debe proporcionar fechaInicio y fechaFin" });
                }

                var logs = await _logService.ObtenerLogsPorRangoFechaAsync(fechaInicio.Value, fechaFin.Value);
                
                return Ok(new
                {
                    fechaInicio,
                    fechaFin,
                    total = logs.Count,
                    datos = logs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs por fecha");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de los logs
        /// </summary>
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            try
            {
                var stats = await _logService.ObtenerEstadisticasAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Limpia logs antiguos
        /// </summary>
        [HttpDelete("limpiar")]
        public async Task<IActionResult> LimpiarLogsAntiguos([FromQuery] int diasAntiguedad = 30)
        {
            try
            {
                var cantidad = await _logService.LimpiarLogsAntiguosAsync(diasAntiguedad);
                
                return Ok(new 
                { 
                    mensaje = $"Se eliminaron {cantidad} logs con más de {diasAntiguedad} días" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar logs");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}
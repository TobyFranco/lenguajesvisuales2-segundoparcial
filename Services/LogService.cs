using Microsoft.EntityFrameworkCore;
using ClienteAPI.Data;
using ClienteAPI.Models;
using ClienteAPI.DTOs;

namespace ClienteAPI.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LogService> _logger;

        public LogService(ApplicationDbContext context, ILogger<LogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RegistrarLogAsync(LogApi log)
        {
            try
            {
                _context.LogsApi.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar log en base de datos");
            }
        }

        public async Task<List<LogApi>> ObtenerLogsAsync(int pagina, int registrosPorPagina, string? tipoLog)
        {
            var query = _context.LogsApi.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tipoLog))
            {
                query = query.Where(l => l.TipoLog == tipoLog);
            }

            return await query
                .OrderByDescending(l => l.DateTime)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();
        }

        public async Task<LogApi?> ObtenerLogPorIdAsync(int id)
        {
            return await _context.LogsApi.FindAsync(id);
        }

        public async Task<List<LogApi>> ObtenerLogsPorRangoFechaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.LogsApi
                .Where(l => l.DateTime >= fechaInicio && l.DateTime <= fechaFin)
                .OrderByDescending(l => l.DateTime)
                .ToListAsync();
        }

        public async Task<EstadisticasLogDto> ObtenerEstadisticasAsync()
        {
            var logs = await _context.LogsApi.ToListAsync();

            var estadisticas = new EstadisticasLogDto
            {
                TotalLogs = logs.Count,
                LogsInfo = logs.Count(l => l.TipoLog == "Info"),
                LogsError = logs.Count(l => l.TipoLog == "Error"),
                LogsWarning = logs.Count(l => l.TipoLog == "Warning"),
                UltimoLog = logs.Any() ? logs.Max(l => l.DateTime) : null,
                LogsPorEndpoint = logs
                    .GroupBy(l => l.UrlEndpoint ?? "Desconocido")
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return estadisticas;
        }

        public async Task<int> LimpiarLogsAntiguosAsync(int diasAntiguedad)
        {
            var fechaLimite = DateTime.Now.AddDays(-diasAntiguedad);

            var logsAntiguos = await _context.LogsApi
                .Where(l => l.DateTime < fechaLimite)
                .ToListAsync();

            _context.LogsApi.RemoveRange(logsAntiguos);
            await _context.SaveChangesAsync();

            return logsAntiguos.Count;
        }
    }
}
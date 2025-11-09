using ClienteAPI.Models;
using ClienteAPI.DTOs;

namespace ClienteAPI.Services
{
    public interface ILogService
    {
        Task RegistrarLogAsync(LogApi log);
        Task<List<LogApi>> ObtenerLogsAsync(int pagina, int registrosPorPagina, string? tipoLog);
        Task<LogApi?> ObtenerLogPorIdAsync(int id);
        Task<List<LogApi>> ObtenerLogsPorRangoFechaAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<EstadisticasLogDto> ObtenerEstadisticasAsync();
        Task<int> LimpiarLogsAntiguosAsync(int diasAntiguedad);
    }
}
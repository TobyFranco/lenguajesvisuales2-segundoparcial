using ClienteAPI.DTOs;

namespace ClienteAPI.Services
{
    public interface IArchivoService
    {
        Task<ResultadoOperacion<List<ArchivoResponseDto>>> CargarArchivosDesdeZipAsync(string ci, IFormFile archivoZip);
        Task<List<ArchivoResponseDto>> ObtenerArchivosPorClienteAsync(string ci);
        Task<(Stream? stream, string nombreArchivo, string contentType)> ObtenerArchivoFisicoAsync(int idArchivo);
        Task<bool> EliminarArchivoAsync(int idArchivo);
    }
}
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using ClienteAPI.Data;
using ClienteAPI.Models;
using ClienteAPI.DTOs;

namespace ClienteAPI.Services
{
    public class ArchivoService : IArchivoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ArchivoService> _logger;

        public ArchivoService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<ArchivoService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<ResultadoOperacion<List<ArchivoResponseDto>>> CargarArchivosDesdeZipAsync(
            string ci, IFormFile archivoZip)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(ci);
                if (cliente == null)
                {
                    return new ResultadoOperacion<List<ArchivoResponseDto>>
                    {
                        Exito = false,
                        Mensaje = $"Cliente con CI {ci} no encontrado"
                    };
                }

                var carpetaTemporal = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                var carpetaDestino = Path.Combine(_environment.ContentRootPath, "Uploads", "Archivos", ci);
                
                Directory.CreateDirectory(carpetaTemporal);
                Directory.CreateDirectory(carpetaDestino);

                var archivosTempZip = Path.Combine(carpetaTemporal, archivoZip.FileName);

                using (var stream = new FileStream(archivosTempZip, FileMode.Create))
                {
                    await archivoZip.CopyToAsync(stream);
                }

                ZipFile.ExtractToDirectory(archivosTempZip, carpetaTemporal);

                var archivosRegistrados = new List<ArchivoResponseDto>();

                var archivos = Directory.GetFiles(carpetaTemporal, "*.*", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

                foreach (var rutaArchivoTemp in archivos)
                {
                    var nombreArchivo = Path.GetFileName(rutaArchivoTemp);
                    var nombreUnico = $"{DateTime.Now:yyyyMMddHHmmss}_{nombreArchivo}";
                    var rutaDestino = Path.Combine(carpetaDestino, nombreUnico);

                    File.Copy(rutaArchivoTemp, rutaDestino, true);

                    var fileInfo = new FileInfo(rutaDestino);

                    var archivoCliente = new ArchivoCliente
                    {
                        CICliente = ci,
                        NombreArchivo = nombreArchivo,
                        UrlArchivo = rutaDestino,
                        TipoArchivo = fileInfo.Extension,
                        TamanoBytes = fileInfo.Length
                    };

                    _context.ArchivosCliente.Add(archivoCliente);
                    await _context.SaveChangesAsync();

                    archivosRegistrados.Add(MapearAArchivoResponse(archivoCliente));
                }

                Directory.Delete(carpetaTemporal, true);

                return new ResultadoOperacion<List<ArchivoResponseDto>>
                {
                    Exito = true,
                    Mensaje = $"Se cargaron {archivosRegistrados.Count} archivos exitosamente",
                    Data = archivosRegistrados
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar archivos desde ZIP");
                return new ResultadoOperacion<List<ArchivoResponseDto>>
                {
                    Exito = false,
                    Mensaje = $"Error al procesar archivos: {ex.Message}"
                };
            }
        }

        public async Task<List<ArchivoResponseDto>> ObtenerArchivosPorClienteAsync(string ci)
        {
            var archivos = await _context.ArchivosCliente
                .Where(a => a.CICliente == ci)
                .OrderByDescending(a => a.FechaCarga)
                .ToListAsync();

            return archivos.Select(MapearAArchivoResponse).ToList();
        }

        public async Task<(Stream? stream, string nombreArchivo, string contentType)> ObtenerArchivoFisicoAsync(
            int idArchivo)
        {
            var archivo = await _context.ArchivosCliente.FindAsync(idArchivo);
            
            if (archivo == null || !File.Exists(archivo.UrlArchivo))
            {
                return (null, string.Empty, string.Empty);
            }

            var stream = new FileStream(archivo.UrlArchivo, FileMode.Open, FileAccess.Read);
            var contentType = ObtenerContentType(archivo.TipoArchivo ?? "");

            return (stream, archivo.NombreArchivo, contentType);
        }

        public async Task<bool> EliminarArchivoAsync(int idArchivo)
        {
            var archivo = await _context.ArchivosCliente.FindAsync(idArchivo);
            if (archivo == null) return false;

            if (File.Exists(archivo.UrlArchivo))
            {
                File.Delete(archivo.UrlArchivo);
            }

            _context.ArchivosCliente.Remove(archivo);
            await _context.SaveChangesAsync();

            return true;
        }

        private ArchivoResponseDto MapearAArchivoResponse(ArchivoCliente archivo)
        {
            return new ArchivoResponseDto
            {
                IdArchivo = archivo.IdArchivo,
                CICliente = archivo.CICliente,
                NombreArchivo = archivo.NombreArchivo,
                UrlArchivo = archivo.UrlArchivo,
                TipoArchivo = archivo.TipoArchivo,
                TamanoBytes = archivo.TamanoBytes,
                TamanoLegible = FormatearTamano(archivo.TamanoBytes),
                FechaCarga = archivo.FechaCarga
            };
        }

        private string FormatearTamano(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private string ObtenerContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                _ => "application/octet-stream"
            };
        }
    }
}
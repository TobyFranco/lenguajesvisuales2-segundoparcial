using Microsoft.EntityFrameworkCore;
using ClienteAPI.Data;
using ClienteAPI.Models;
using ClienteAPI.DTOs;

namespace ClienteAPI.Services
{
    public class ClienteService : IClienteService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<ClienteService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<ResultadoOperacion<ClienteResponseDto>> RegistrarClienteAsync(ClienteRegistroDto dto)
        {
            try
            {
                var existente = await _context.Clientes.FindAsync(dto.CI);
                if (existente != null)
                {
                    return new ResultadoOperacion<ClienteResponseDto>
                    {
                        Exito = false,
                        Mensaje = $"Ya existe un cliente con CI {dto.CI}"
                    };
                }

                var carpetaFotos = Path.Combine(_environment.ContentRootPath, "Uploads", "Fotos", dto.CI);
                Directory.CreateDirectory(carpetaFotos);

                var cliente = new Cliente
                {
                    CI = dto.CI,
                    Nombres = dto.Nombres,
                    Direccion = dto.Direccion,
                    Telefono = dto.Telefono
                };

                cliente.FotoCasa1 = await GuardarFotoAsync(dto.FotoCasa1, carpetaFotos, "FotoCasa1");
                cliente.FotoCasa2 = await GuardarFotoAsync(dto.FotoCasa2, carpetaFotos, "FotoCasa2");
                cliente.FotoCasa3 = await GuardarFotoAsync(dto.FotoCasa3, carpetaFotos, "FotoCasa3");

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                return new ResultadoOperacion<ClienteResponseDto>
                {
                    Exito = true,
                    Mensaje = "Cliente registrado exitosamente",
                    Data = MapearAClienteResponse(cliente)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar cliente");
                return new ResultadoOperacion<ClienteResponseDto>
                {
                    Exito = false,
                    Mensaje = $"Error al registrar cliente: {ex.Message}"
                };
            }
        }

        public async Task<ClienteResponseDto?> ObtenerClientePorCIAsync(string ci)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Archivos)
                .FirstOrDefaultAsync(c => c.CI == ci);

            return cliente == null ? null : MapearAClienteResponse(cliente);
        }

        public async Task<List<ClienteResponseDto>> ListarClientesAsync()
        {
            var clientes = await _context.Clientes
                .Include(c => c.Archivos)
                .OrderByDescending(c => c.FechaRegistro)
                .ToListAsync();

            return clientes.Select(MapearAClienteResponse).ToList();
        }

        public async Task<bool> EliminarClienteAsync(string ci)
        {
            var cliente = await _context.Clientes.FindAsync(ci);
            if (cliente == null) return false;

            var carpetaCliente = Path.Combine(_environment.ContentRootPath, "Uploads", "Fotos", ci);
            if (Directory.Exists(carpetaCliente))
            {
                Directory.Delete(carpetaCliente, true);
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<string?> GuardarFotoAsync(IFormFile? foto, string carpeta, string nombreBase)
        {
            if (foto == null || foto.Length == 0) return null;

            var extension = Path.GetExtension(foto.FileName);
            var nombreArchivo = $"{nombreBase}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await foto.CopyToAsync(stream);
            }

            return rutaCompleta;
        }

        private ClienteResponseDto MapearAClienteResponse(Cliente cliente)
        {
            return new ClienteResponseDto
            {
                CI = cliente.CI,
                Nombres = cliente.Nombres,
                Direccion = cliente.Direccion,
                Telefono = cliente.Telefono,
                FotoCasa1 = cliente.FotoCasa1,
                FotoCasa2 = cliente.FotoCasa2,
                FotoCasa3 = cliente.FotoCasa3,
                FechaRegistro = cliente.FechaRegistro,
                CantidadArchivos = cliente.Archivos?.Count ?? 0
            };
        }
    }
}
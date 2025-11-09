using ClienteAPI.DTOs;

namespace ClienteAPI.Services
{
    public interface IClienteService
    {
        Task<ResultadoOperacion<ClienteResponseDto>> RegistrarClienteAsync(ClienteRegistroDto dto);
        Task<ClienteResponseDto?> ObtenerClientePorCIAsync(string ci);
        Task<List<ClienteResponseDto>> ListarClientesAsync();
        Task<bool> EliminarClienteAsync(string ci);
    }
}
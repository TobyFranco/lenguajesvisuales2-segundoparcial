using Microsoft.AspNetCore.Mvc;
using ClienteAPI.Services;
using ClienteAPI.DTOs;

namespace ClienteAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<ClienteController> _logger;

        public ClienteController(IClienteService clienteService, ILogger<ClienteController> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        /// <summary>
        /// Registra un nuevo cliente con sus fotografías
        /// </summary>
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarCliente([FromForm] ClienteRegistroDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { mensaje = "Datos inválidos", errores = ModelState });
                }

                var resultado = await _clienteService.RegistrarClienteAsync(dto);

                if (!resultado.Exito)
                {
                    return BadRequest(new { mensaje = resultado.Mensaje });
                }

                return Ok(new 
                { 
                    mensaje = "Cliente registrado exitosamente",
                    cliente = resultado.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar cliente");
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene la información de un cliente por CI
        /// </summary>
        [HttpGet("{ci}")]
        public async Task<IActionResult> ObtenerCliente(string ci)
        {
            try
            {
                var cliente = await _clienteService.ObtenerClientePorCIAsync(ci);

                if (cliente == null)
                {
                    return NotFound(new { mensaje = $"Cliente con CI {ci} no encontrado" });
                }

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cliente");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Lista todos los clientes
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListarClientes()
        {
            try
            {
                var clientes = await _clienteService.ListarClientesAsync();
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar clientes");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina un cliente
        /// </summary>
        [HttpDelete("{ci}")]
        public async Task<IActionResult> EliminarCliente(string ci)
        {
            try
            {
                var resultado = await _clienteService.EliminarClienteAsync(ci);

                if (!resultado)
                {
                    return NotFound(new { mensaje = $"Cliente con CI {ci} no encontrado" });
                }

                return Ok(new { mensaje = "Cliente eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}
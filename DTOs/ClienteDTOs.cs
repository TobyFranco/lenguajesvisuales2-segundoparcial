using System.ComponentModel.DataAnnotations;

namespace ClienteAPI.DTOs
{
    public class ClienteRegistroDto
    {
        [Required(ErrorMessage = "El CI es obligatorio")]
        [MaxLength(20)]
        public string CI { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [MaxLength(200)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [MaxLength(300)]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        public IFormFile? FotoCasa1 { get; set; }
        public IFormFile? FotoCasa2 { get; set; }
        public IFormFile? FotoCasa3 { get; set; }
    }

    public class ClienteResponseDto
    {
        public string CI { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? FotoCasa1 { get; set; }
        public string? FotoCasa2 { get; set; }
        public string? FotoCasa3 { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int CantidadArchivos { get; set; }
    }

    public class ArchivoResponseDto
    {
        public int IdArchivo { get; set; }
        public string CICliente { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public string UrlArchivo { get; set; } = string.Empty;
        public string? TipoArchivo { get; set; }
        public long TamanoBytes { get; set; }
        public string TamanoLegible { get; set; } = string.Empty;
        public DateTime FechaCarga { get; set; }
    }

    public class ResultadoOperacion<T>
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class EstadisticasLogDto
    {
        public int TotalLogs { get; set; }
        public int LogsInfo { get; set; }
        public int LogsError { get; set; }
        public int LogsWarning { get; set; }
        public DateTime? UltimoLog { get; set; }
        public Dictionary<string, int> LogsPorEndpoint { get; set; } = new();
    }
}
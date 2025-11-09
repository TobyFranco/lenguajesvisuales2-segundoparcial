using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClienteAPI.Models
{
    public class Cliente
    {
        [Key]
        [MaxLength(20)]
        public string CI { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? FotoCasa1 { get; set; }

        [MaxLength(500)]
        public string? FotoCasa2 { get; set; }

        [MaxLength(500)]
        public string? FotoCasa3 { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public virtual ICollection<ArchivoCliente> Archivos { get; set; } = new List<ArchivoCliente>();
    }

    public class ArchivoCliente
    {
        [Key]
        public int IdArchivo { get; set; }

        [Required]
        [MaxLength(20)]
        public string CICliente { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string NombreArchivo { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string UrlArchivo { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? TipoArchivo { get; set; }

        public long TamanoBytes { get; set; }

        public DateTime FechaCarga { get; set; } = DateTime.Now;

        [ForeignKey("CICliente")]
        public virtual Cliente? Cliente { get; set; }
    }

    public class LogApi
    {
        [Key]
        public int IdLog { get; set; }

        public DateTime DateTime { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string TipoLog { get; set; } = string.Empty;

        public string? RequestBody { get; set; }

        public string? ResponseBody { get; set; }

        [MaxLength(500)]
        public string? UrlEndpoint { get; set; }

        [MaxLength(10)]
        public string? MetodoHttp { get; set; }

        [MaxLength(50)]
        public string? DireccionIp { get; set; }

        public string? Detalle { get; set; }

        public int? StatusCode { get; set; }
    }
}
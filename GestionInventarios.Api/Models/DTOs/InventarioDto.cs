using System.ComponentModel.DataAnnotations;

namespace GestionInventarios.Api.Models.DTOs
{
    public class InventarioCreateDto
    {
        [Required, MaxLength(80)]
        public string Serial { get; set; } = string.Empty;

        [Required]
        public int IdMarca { get; set; }

        [Required]
        public int IdTipo { get; set; }

        [Required]
        public int IdEstado { get; set; }

        [MaxLength(255)]
        public string? Descripcion { get; set; }

        [Required]
        public DateTime FechaIngreso { get; set; }

        [MaxLength(150)]
        public string? Ubicacion { get; set; }

        public bool Activo { get; set; } = true;
    }

    public class InventarioResponseDto
    {
        public int      IdInventario { get; set; }
        public string   Serial       { get; set; } = string.Empty;
        public int      IdMarca      { get; set; }
        public string?  MarcaNombre  { get; set; }
        public int      IdTipo       { get; set; }
        public string?  TipoNombre   { get; set; }
        public int      IdEstado     { get; set; }
        public string?  EstadoNombre { get; set; }
        public string?  Descripcion  { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string?  Ubicacion    { get; set; }
        public bool     Activo       { get; set; }
    }
}

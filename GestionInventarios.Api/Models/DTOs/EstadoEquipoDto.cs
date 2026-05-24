using System.ComponentModel.DataAnnotations;

namespace GestionInventarios.Api.Models.DTOs
{
    public class EstadoEquipoCreateDto
    {
        [Required, MaxLength(80)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }

    public class EstadoEquipoResponseDto
    {
        public int     IdEstado    { get; set; }
        public string  Nombre      { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool    Activo      { get; set; }
    }
}

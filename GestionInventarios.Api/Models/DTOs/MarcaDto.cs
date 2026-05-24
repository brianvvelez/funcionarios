using System.ComponentModel.DataAnnotations;

namespace GestionInventarios.Api.Models.DTOs
{
    public class MarcaCreateDto
    {
        [Required, MaxLength(80)]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }

    public class MarcaResponseDto
    {
        public int    IdMarca { get; set; }
        public string Nombre  { get; set; } = string.Empty;
        public bool   Activo  { get; set; }
    }
}

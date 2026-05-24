using System.ComponentModel.DataAnnotations;

namespace GestionInventarios.Api.Models.DTOs
{
    public class UsuarioCreateDto
    {
        [Required, MaxLength(100)]
        public string Nombres { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Apellidos { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required, RegularExpression("^(Administrador|Docente)$",
            ErrorMessage = "El rol debe ser 'Administrador' o 'Docente'.")]
        public string Rol { get; set; } = string.Empty;
    }
}

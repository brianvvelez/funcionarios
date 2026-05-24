using System.ComponentModel.DataAnnotations;

namespace GestionInventarios.Api.Models.DTOs
{
    public class UsuarioUpdateDto
    {
        [Required, MaxLength(100)]
        public string Nombres { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Apellidos { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, RegularExpression("^(Administrador|Docente)$",
            ErrorMessage = "El rol debe ser 'Administrador' o 'Docente'.")]
        public string Rol { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        /// <summary>
        /// Opcional. Si se envía, se actualiza el hash de la contraseña.
        /// Si viene null o vacío, el password actual se conserva.
        /// </summary>
        [MinLength(6), MaxLength(100)]
        public string? Password { get; set; }
    }
}

namespace GestionInventarios.Api.Models.DTOs
{
    /// <summary>
    /// DTO de respuesta para usuarios. NO incluye password_hash bajo
    /// ningún concepto, ya que se devuelve por HTTP.
    /// </summary>
    public class UsuarioResponseDto
    {
        public int      IdUsuario     { get; set; }
        public string   Nombres       { get; set; } = string.Empty;
        public string   Apellidos     { get; set; } = string.Empty;
        public string   Email         { get; set; } = string.Empty;
        public string   Rol           { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool     Activo        { get; set; }
    }
}

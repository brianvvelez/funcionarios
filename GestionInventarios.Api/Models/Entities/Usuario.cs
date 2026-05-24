namespace GestionInventarios.Api.Models.Entities
{
    /// <summary>
    /// Entidad que representa un usuario del sistema. Mapea la tabla
    /// 'usuarios'. El campo PasswordHash NUNCA debe exponerse al exterior:
    /// se guarda como hash BCrypt y sólo se usa internamente para
    /// validar credenciales en el login.
    /// </summary>
    public class Usuario
    {
        public int      IdUsuario      { get; set; }
        public string   Nombres        { get; set; } = string.Empty;
        public string   Apellidos      { get; set; } = string.Empty;
        public string   Email          { get; set; } = string.Empty;
        public string   PasswordHash   { get; set; } = string.Empty;
        public string   Rol            { get; set; } = string.Empty;
        public DateTime FechaCreacion  { get; set; }
        public bool     Activo         { get; set; } = true;
    }
}

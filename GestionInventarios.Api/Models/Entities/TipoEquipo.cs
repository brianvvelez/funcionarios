namespace GestionInventarios.Api.Models.Entities
{
    /// <summary>
    /// Catálogo de tipos de equipos (Computador, Impresora, Proyector...).
    /// Mapea la tabla 'tipos_equipos'.
    /// </summary>
    public class TipoEquipo
    {
        public int     IdTipo      { get; set; }
        public string  Nombre      { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool    Activo      { get; set; } = true;
    }
}

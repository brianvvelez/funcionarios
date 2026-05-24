namespace GestionInventarios.Api.Models.Entities
{
    /// <summary>
    /// Catálogo de estados que puede tener un equipo del inventario.
    /// Mapea la tabla 'estados_equipos'.
    /// </summary>
    public class EstadoEquipo
    {
        public int     IdEstado    { get; set; }
        public string  Nombre      { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool    Activo      { get; set; } = true;
    }
}

namespace GestionInventarios.Api.Models.Entities
{
    /// <summary>
    /// Entidad principal del módulo de inventarios. Cada equipo
    /// registrado tiene un serial único y referencia a marca,
    /// tipo y estado mediante claves foráneas.
    /// </summary>
    public class Inventario
    {
        public int      IdInventario { get; set; }
        public string   Serial       { get; set; } = string.Empty;
        public int      IdMarca      { get; set; }
        public int      IdTipo       { get; set; }
        public int      IdEstado     { get; set; }
        public string?  Descripcion  { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string?  Ubicacion    { get; set; }
        public bool     Activo       { get; set; } = true;

        // Campos calculados / de visualización (provienen de los JOIN).
        public string? MarcaNombre  { get; set; }
        public string? TipoNombre   { get; set; }
        public string? EstadoNombre { get; set; }
    }
}

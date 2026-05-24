namespace GestionInventarios.Api.Models.Entities
{
    /// <summary>
    /// Catálogo de marcas de equipos. Mapea la tabla 'marcas'.
    /// </summary>
    public class Marca
    {
        public int    IdMarca { get; set; }
        public string Nombre  { get; set; } = string.Empty;
        public bool   Activo  { get; set; } = true;
    }
}

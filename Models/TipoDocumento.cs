namespace GestionFuncionarios.Models
{
    /// <summary>
    /// Tipo de documento de identificación (CC, CE, TI, PA...).
    /// </summary>
    public class TipoDocumento
    {
        public int    IdTipoDocumento { get; set; }
        public string Codigo          { get; set; } = string.Empty;
        public string Descripcion     { get; set; } = string.Empty;

        // ToString se usa para mostrar el item en los ComboBox.
        public override string ToString() => $"{Codigo} - {Descripcion}";
    }
}

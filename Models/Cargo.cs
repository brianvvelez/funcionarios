namespace GestionFuncionarios.Models
{
    /// <summary>
    /// Cargo o rol que ocupa un funcionario.
    /// </summary>
    public class Cargo
    {
        public int     IdCargo     { get; set; }
        public string  Nombre      { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal SalarioBase { get; set; }

        public override string ToString() => Nombre;
    }
}

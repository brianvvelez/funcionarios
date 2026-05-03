namespace GestionFuncionarios.Models
{
    /// <summary>
    /// Dependencia, área o departamento al que pertenece un funcionario.
    /// </summary>
    public class Dependencia
    {
        public int     IdDependencia { get; set; }
        public string  Nombre        { get; set; } = string.Empty;
        public string? Descripcion   { get; set; }

        public override string ToString() => Nombre;
    }
}

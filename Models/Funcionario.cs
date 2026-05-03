using System;

namespace GestionFuncionarios.Models
{
    /// <summary>
    /// Entidad que representa un funcionario de la organización.
    /// Mapea la tabla 'funcionarios' de la base de datos.
    /// </summary>
    public class Funcionario
    {
        public int      IdFuncionario    { get; set; }
        public int      IdTipoDocumento  { get; set; }
        public string   NumeroDocumento  { get; set; } = string.Empty;
        public string   Nombres          { get; set; } = string.Empty;
        public string   Apellidos        { get; set; } = string.Empty;
        public string   Email            { get; set; } = string.Empty;
        public string?  Telefono         { get; set; }
        public string?  Direccion        { get; set; }
        public DateTime FechaNacimiento  { get; set; }
        public DateTime FechaIngreso     { get; set; }
        public int      IdCargo          { get; set; }
        public int      IdDependencia    { get; set; }
        public decimal  Salario          { get; set; }
        public bool     Activo           { get; set; } = true;

        // Campos calculados / de visualización (provienen de los JOIN).
        public string?  TipoDocumentoDescripcion { get; set; }
        public string?  CargoNombre              { get; set; }
        public string?  DependenciaNombre        { get; set; }

        public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
    }
}

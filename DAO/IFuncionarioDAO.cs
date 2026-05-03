using System.Collections.Generic;
using GestionFuncionarios.Models;

namespace GestionFuncionarios.DAO
{
    /// <summary>
    /// Contrato del Data Access Object (DAO) para la entidad Funcionario.
    /// Aplica el patrón DAO: la lógica de negocio y de presentación NO
    /// conoce los detalles de persistencia, sólo esta interfaz.
    /// </summary>
    public interface IFuncionarioDAO
    {
        /// <summary>Devuelve el listado de funcionarios incluyendo sus relaciones.</summary>
        List<Funcionario> Listar();

        /// <summary>Obtiene un funcionario por su identificador, o null si no existe.</summary>
        Funcionario? ObtenerPorId(int idFuncionario);

        /// <summary>Inserta un nuevo funcionario y retorna el id generado.</summary>
        int Crear(Funcionario funcionario);

        /// <summary>Actualiza un funcionario existente. Retorna true si se modificó algún registro.</summary>
        bool Actualizar(Funcionario funcionario);

        /// <summary>Elimina (físicamente) un funcionario por su id. Retorna true si se eliminó.</summary>
        bool Eliminar(int idFuncionario);
    }
}

using GestionInventarios.Api.Models.Entities;

namespace GestionInventarios.Api.DAO.Interfaces
{
    /// <summary>Contrato DAO para la entidad EstadoEquipo.</summary>
    public interface IEstadoEquipoDAO
    {
        List<EstadoEquipo> Listar();
        EstadoEquipo? ObtenerPorId(int idEstado);
        int Crear(EstadoEquipo estado);
        bool Actualizar(EstadoEquipo estado);
        bool Eliminar(int idEstado);

        /// <summary>Indica si la tabla está vacía (apoyo al seeder).</summary>
        bool EstaVacia();
    }
}

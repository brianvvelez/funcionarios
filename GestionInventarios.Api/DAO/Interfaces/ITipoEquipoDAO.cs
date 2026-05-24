using GestionInventarios.Api.Models.Entities;

namespace GestionInventarios.Api.DAO.Interfaces
{
    /// <summary>Contrato DAO para la entidad TipoEquipo.</summary>
    public interface ITipoEquipoDAO
    {
        List<TipoEquipo> Listar();
        TipoEquipo? ObtenerPorId(int idTipo);
        int Crear(TipoEquipo tipo);
        bool Actualizar(TipoEquipo tipo);
        bool Eliminar(int idTipo);
    }
}

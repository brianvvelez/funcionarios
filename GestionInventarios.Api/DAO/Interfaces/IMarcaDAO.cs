using GestionInventarios.Api.Models.Entities;

namespace GestionInventarios.Api.DAO.Interfaces
{
    /// <summary>Contrato DAO para la entidad Marca.</summary>
    public interface IMarcaDAO
    {
        List<Marca> Listar();
        Marca? ObtenerPorId(int idMarca);
        int Crear(Marca marca);
        bool Actualizar(Marca marca);
        bool Eliminar(int idMarca);
    }
}

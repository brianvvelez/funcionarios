using GestionInventarios.Api.Models.Entities;

namespace GestionInventarios.Api.DAO.Interfaces
{
    /// <summary>Contrato DAO para la entidad Inventario.</summary>
    public interface IInventarioDAO
    {
        /// <summary>Lista los inventarios incluyendo nombres de marca, tipo y estado (JOIN).</summary>
        List<Inventario> Listar();
        Inventario? ObtenerPorId(int idInventario);
        int Crear(Inventario inventario);
        bool Actualizar(Inventario inventario);
        bool Eliminar(int idInventario);
    }
}

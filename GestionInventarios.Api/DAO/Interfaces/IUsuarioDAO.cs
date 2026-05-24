using GestionInventarios.Api.Models.Entities;

namespace GestionInventarios.Api.DAO.Interfaces
{
    /// <summary>
    /// Contrato del Data Access Object (DAO) para la entidad Usuario.
    /// Aplica el patrón DAO: las capas superiores (Services, Controllers)
    /// no conocen detalles de persistencia, sólo esta interfaz.
    /// </summary>
    public interface IUsuarioDAO
    {
        /// <summary>Devuelve el listado de usuarios.</summary>
        List<Usuario> Listar();

        /// <summary>Obtiene un usuario por su identificador, o null si no existe.</summary>
        Usuario? ObtenerPorId(int idUsuario);

        /// <summary>Obtiene un usuario por su email, o null si no existe.</summary>
        Usuario? ObtenerPorEmail(string email);

        /// <summary>Inserta un nuevo usuario y retorna el id generado.</summary>
        int Crear(Usuario usuario);

        /// <summary>Actualiza un usuario existente. Retorna true si se modificó algún registro.</summary>
        bool Actualizar(Usuario usuario);

        /// <summary>Elimina un usuario por su id. Retorna true si se eliminó.</summary>
        bool Eliminar(int idUsuario);

        /// <summary>Indica si la tabla usuarios está vacía. Útil para el seeder de arranque.</summary>
        bool EstaVacia();
    }
}

using System.Data;
using Microsoft.Data.SqlClient;
using GestionInventarios.Api.DAO.Interfaces;
using GestionInventarios.Api.Exceptions;
using GestionInventarios.Api.Models.Entities;
using GestionInventarios.Api.Util;

namespace GestionInventarios.Api.DAO.Implementations
{
    /// <summary>
    /// Implementación del DAO de Usuario sobre SQL Server usando ADO.NET
    /// (Microsoft.Data.SqlClient). Toda excepción técnica se envuelve en
    /// DAOException para no exponer detalles internos.
    /// </summary>
    public class UsuarioDAO : IUsuarioDAO
    {
        // Códigos de error comunes de SQL Server.
        private const int ERR_UNIQUE_PK    = 2627;
        private const int ERR_UNIQUE_INDEX = 2601;
        private const int ERR_FK_CONSTRAINT = 547;

        private readonly ConexionDB _conexion;

        public UsuarioDAO(ConexionDB conexion) => _conexion = conexion;

        public List<Usuario> Listar()
        {
            var lista = new List<Usuario>();

            const string sql = @"
                SELECT id_usuario, nombres, apellidos, email, password_hash,
                       rol, fecha_creacion, activo
                FROM   usuarios
                ORDER BY apellidos, nombres;";

            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                using SqlDataReader r    = cmd.ExecuteReader();

                while (r.Read())
                {
                    lista.Add(Mapear(r));
                }
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al consultar el listado de usuarios.", ex);
            }

            return lista;
        }

        public Usuario? ObtenerPorId(int idUsuario)
        {
            const string sql = @"
                SELECT id_usuario, nombres, apellidos, email, password_hash,
                       rol, fecha_creacion, activo
                FROM   usuarios
                WHERE  id_usuario = @id;";

            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;

                using SqlDataReader r = cmd.ExecuteReader();
                return r.Read() ? Mapear(r) : null;
            }
            catch (SqlException ex)
            {
                throw new DAOException(
                    $"Error al consultar el usuario con id {idUsuario}.", ex);
            }
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            const string sql = @"
                SELECT id_usuario, nombres, apellidos, email, password_hash,
                       rol, fecha_creacion, activo
                FROM   usuarios
                WHERE  email = @email;";

            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@email", SqlDbType.VarChar, 150).Value = email;

                using SqlDataReader r = cmd.ExecuteReader();
                return r.Read() ? Mapear(r) : null;
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al consultar el usuario por email.", ex);
            }
        }

        public int Crear(Usuario u)
        {
            const string sql = @"
                INSERT INTO usuarios (nombres, apellidos, email, password_hash, rol, activo)
                OUTPUT INSERTED.id_usuario
                VALUES (@nombres, @apellidos, @email, @passwordHash, @rol, @activo);";

            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@nombres",      SqlDbType.VarChar, 100).Value = u.Nombres;
                cmd.Parameters.Add("@apellidos",    SqlDbType.VarChar, 100).Value = u.Apellidos;
                cmd.Parameters.Add("@email",        SqlDbType.VarChar, 150).Value = u.Email;
                cmd.Parameters.Add("@passwordHash", SqlDbType.VarChar, 255).Value = u.PasswordHash;
                cmd.Parameters.Add("@rol",          SqlDbType.VarChar, 20).Value  = u.Rol;
                cmd.Parameters.Add("@activo",       SqlDbType.Bit).Value          = u.Activo;

                object? result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_UNIQUE_PK || ex.Number == ERR_UNIQUE_INDEX)
            {
                throw new DAOException(
                    "Ya existe un usuario registrado con ese correo electrónico.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al crear el usuario.", ex);
            }
        }

        public bool Actualizar(Usuario u)
        {
            const string sql = @"
                UPDATE usuarios SET
                    nombres       = @nombres,
                    apellidos     = @apellidos,
                    email         = @email,
                    password_hash = @passwordHash,
                    rol           = @rol,
                    activo        = @activo
                WHERE id_usuario  = @id;";

            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@nombres",      SqlDbType.VarChar, 100).Value = u.Nombres;
                cmd.Parameters.Add("@apellidos",    SqlDbType.VarChar, 100).Value = u.Apellidos;
                cmd.Parameters.Add("@email",        SqlDbType.VarChar, 150).Value = u.Email;
                cmd.Parameters.Add("@passwordHash", SqlDbType.VarChar, 255).Value = u.PasswordHash;
                cmd.Parameters.Add("@rol",          SqlDbType.VarChar, 20).Value  = u.Rol;
                cmd.Parameters.Add("@activo",       SqlDbType.Bit).Value          = u.Activo;
                cmd.Parameters.Add("@id",           SqlDbType.Int).Value          = u.IdUsuario;

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_UNIQUE_PK || ex.Number == ERR_UNIQUE_INDEX)
            {
                throw new DAOException(
                    "Ya existe otro usuario registrado con ese correo electrónico.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException(
                    $"Error al actualizar el usuario con id {u.IdUsuario}.", ex);
            }
        }

        public bool Eliminar(int idUsuario)
        {
            const string sql = @"DELETE FROM usuarios WHERE id_usuario = @id;";

            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_FK_CONSTRAINT)
            {
                throw new DAOException(
                    "No se puede eliminar el usuario porque tiene registros asociados.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException(
                    $"Error al eliminar el usuario con id {idUsuario}.", ex);
            }
        }

        public bool EstaVacia()
        {
            const string sql = @"SELECT COUNT(*) FROM usuarios;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al verificar si la tabla de usuarios está vacía.", ex);
            }
        }

        private static Usuario Mapear(SqlDataReader r) => new()
        {
            IdUsuario     = r.GetInt32   (r.GetOrdinal("id_usuario")),
            Nombres       = r.GetString  (r.GetOrdinal("nombres")),
            Apellidos     = r.GetString  (r.GetOrdinal("apellidos")),
            Email         = r.GetString  (r.GetOrdinal("email")),
            PasswordHash  = r.GetString  (r.GetOrdinal("password_hash")),
            Rol           = r.GetString  (r.GetOrdinal("rol")),
            FechaCreacion = r.GetDateTime(r.GetOrdinal("fecha_creacion")),
            Activo        = r.GetBoolean (r.GetOrdinal("activo"))
        };
    }
}

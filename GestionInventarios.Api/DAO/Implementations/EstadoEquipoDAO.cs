using System.Data;
using Microsoft.Data.SqlClient;
using GestionInventarios.Api.DAO.Interfaces;
using GestionInventarios.Api.Exceptions;
using GestionInventarios.Api.Models.Entities;
using GestionInventarios.Api.Util;

namespace GestionInventarios.Api.DAO.Implementations
{
    /// <summary>Implementación DAO para EstadoEquipo (ADO.NET).</summary>
    public class EstadoEquipoDAO : IEstadoEquipoDAO
    {
        private const int ERR_UNIQUE_PK     = 2627;
        private const int ERR_UNIQUE_INDEX  = 2601;
        private const int ERR_FK_CONSTRAINT = 547;

        private readonly ConexionDB _conexion;

        public EstadoEquipoDAO(ConexionDB conexion) => _conexion = conexion;

        public List<EstadoEquipo> Listar()
        {
            var lista = new List<EstadoEquipo>();
            const string sql = @"SELECT id_estado, nombre, descripcion, activo
                                 FROM estados_equipos ORDER BY nombre;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                using SqlDataReader r    = cmd.ExecuteReader();
                while (r.Read()) lista.Add(Mapear(r));
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al consultar el listado de estados.", ex);
            }
            return lista;
        }

        public EstadoEquipo? ObtenerPorId(int idEstado)
        {
            const string sql = @"SELECT id_estado, nombre, descripcion, activo
                                 FROM estados_equipos WHERE id_estado = @id;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEstado;
                using SqlDataReader r = cmd.ExecuteReader();
                return r.Read() ? Mapear(r) : null;
            }
            catch (SqlException ex)
            {
                throw new DAOException($"Error al consultar el estado con id {idEstado}.", ex);
            }
        }

        public int Crear(EstadoEquipo e)
        {
            const string sql = @"
                INSERT INTO estados_equipos (nombre, descripcion, activo)
                OUTPUT INSERTED.id_estado
                VALUES (@nombre, @descripcion, @activo);";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@nombre",      SqlDbType.VarChar, 80).Value  = e.Nombre;
                cmd.Parameters.Add("@descripcion", SqlDbType.VarChar, 255).Value = (object?)e.Descripcion ?? DBNull.Value;
                cmd.Parameters.Add("@activo",      SqlDbType.Bit).Value          = e.Activo;
                object? result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_UNIQUE_PK || ex.Number == ERR_UNIQUE_INDEX)
            {
                throw new DAOException("Ya existe un estado con ese nombre.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al crear el estado.", ex);
            }
        }

        public bool Actualizar(EstadoEquipo e)
        {
            const string sql = @"
                UPDATE estados_equipos SET
                    nombre = @nombre, descripcion = @descripcion, activo = @activo
                WHERE id_estado = @id;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@nombre",      SqlDbType.VarChar, 80).Value  = e.Nombre;
                cmd.Parameters.Add("@descripcion", SqlDbType.VarChar, 255).Value = (object?)e.Descripcion ?? DBNull.Value;
                cmd.Parameters.Add("@activo",      SqlDbType.Bit).Value          = e.Activo;
                cmd.Parameters.Add("@id",          SqlDbType.Int).Value          = e.IdEstado;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_UNIQUE_PK || ex.Number == ERR_UNIQUE_INDEX)
            {
                throw new DAOException("Ya existe otro estado con ese nombre.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException($"Error al actualizar el estado con id {e.IdEstado}.", ex);
            }
        }

        public bool Eliminar(int idEstado)
        {
            const string sql = @"DELETE FROM estados_equipos WHERE id_estado = @id;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEstado;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_FK_CONSTRAINT)
            {
                throw new DAOException(
                    "No se puede eliminar el estado porque tiene inventarios asociados.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException($"Error al eliminar el estado con id {idEstado}.", ex);
            }
        }

        public bool EstaVacia()
        {
            const string sql = @"SELECT COUNT(*) FROM estados_equipos;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al verificar si la tabla de estados está vacía.", ex);
            }
        }

        private static EstadoEquipo Mapear(SqlDataReader r) => new()
        {
            IdEstado    = r.GetInt32  (r.GetOrdinal("id_estado")),
            Nombre      = r.GetString (r.GetOrdinal("nombre")),
            Descripcion = r.IsDBNull(r.GetOrdinal("descripcion")) ? null : r.GetString(r.GetOrdinal("descripcion")),
            Activo      = r.GetBoolean(r.GetOrdinal("activo"))
        };
    }
}

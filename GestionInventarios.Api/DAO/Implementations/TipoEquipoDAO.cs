using System.Data;
using Microsoft.Data.SqlClient;
using GestionInventarios.Api.DAO.Interfaces;
using GestionInventarios.Api.Exceptions;
using GestionInventarios.Api.Models.Entities;
using GestionInventarios.Api.Util;

namespace GestionInventarios.Api.DAO.Implementations
{
    /// <summary>Implementación DAO para TipoEquipo (ADO.NET).</summary>
    public class TipoEquipoDAO : ITipoEquipoDAO
    {
        private const int ERR_UNIQUE_PK     = 2627;
        private const int ERR_UNIQUE_INDEX  = 2601;
        private const int ERR_FK_CONSTRAINT = 547;

        private readonly ConexionDB _conexion;

        public TipoEquipoDAO(ConexionDB conexion) => _conexion = conexion;

        public List<TipoEquipo> Listar()
        {
            var lista = new List<TipoEquipo>();
            const string sql = @"SELECT id_tipo, nombre, descripcion, activo
                                 FROM tipos_equipos ORDER BY nombre;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                using SqlDataReader r    = cmd.ExecuteReader();
                while (r.Read()) lista.Add(Mapear(r));
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al consultar el listado de tipos de equipo.", ex);
            }
            return lista;
        }

        public TipoEquipo? ObtenerPorId(int idTipo)
        {
            const string sql = @"SELECT id_tipo, nombre, descripcion, activo
                                 FROM tipos_equipos WHERE id_tipo = @id;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idTipo;
                using SqlDataReader r = cmd.ExecuteReader();
                return r.Read() ? Mapear(r) : null;
            }
            catch (SqlException ex)
            {
                throw new DAOException($"Error al consultar el tipo con id {idTipo}.", ex);
            }
        }

        public int Crear(TipoEquipo t)
        {
            const string sql = @"
                INSERT INTO tipos_equipos (nombre, descripcion, activo)
                OUTPUT INSERTED.id_tipo
                VALUES (@nombre, @descripcion, @activo);";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@nombre",      SqlDbType.VarChar, 80).Value  = t.Nombre;
                cmd.Parameters.Add("@descripcion", SqlDbType.VarChar, 255).Value = (object?)t.Descripcion ?? DBNull.Value;
                cmd.Parameters.Add("@activo",      SqlDbType.Bit).Value          = t.Activo;
                object? result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_UNIQUE_PK || ex.Number == ERR_UNIQUE_INDEX)
            {
                throw new DAOException("Ya existe un tipo de equipo con ese nombre.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al crear el tipo de equipo.", ex);
            }
        }

        public bool Actualizar(TipoEquipo t)
        {
            const string sql = @"
                UPDATE tipos_equipos SET
                    nombre = @nombre, descripcion = @descripcion, activo = @activo
                WHERE id_tipo = @id;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@nombre",      SqlDbType.VarChar, 80).Value  = t.Nombre;
                cmd.Parameters.Add("@descripcion", SqlDbType.VarChar, 255).Value = (object?)t.Descripcion ?? DBNull.Value;
                cmd.Parameters.Add("@activo",      SqlDbType.Bit).Value          = t.Activo;
                cmd.Parameters.Add("@id",          SqlDbType.Int).Value          = t.IdTipo;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_UNIQUE_PK || ex.Number == ERR_UNIQUE_INDEX)
            {
                throw new DAOException("Ya existe otro tipo con ese nombre.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException($"Error al actualizar el tipo con id {t.IdTipo}.", ex);
            }
        }

        public bool Eliminar(int idTipo)
        {
            const string sql = @"DELETE FROM tipos_equipos WHERE id_tipo = @id;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idTipo;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_FK_CONSTRAINT)
            {
                throw new DAOException(
                    "No se puede eliminar el tipo de equipo porque tiene inventarios asociados.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException($"Error al eliminar el tipo con id {idTipo}.", ex);
            }
        }

        private static TipoEquipo Mapear(SqlDataReader r) => new()
        {
            IdTipo      = r.GetInt32  (r.GetOrdinal("id_tipo")),
            Nombre      = r.GetString (r.GetOrdinal("nombre")),
            Descripcion = r.IsDBNull(r.GetOrdinal("descripcion")) ? null : r.GetString(r.GetOrdinal("descripcion")),
            Activo      = r.GetBoolean(r.GetOrdinal("activo"))
        };
    }
}

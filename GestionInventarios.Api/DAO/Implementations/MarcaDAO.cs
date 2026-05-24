using System.Data;
using Microsoft.Data.SqlClient;
using GestionInventarios.Api.DAO.Interfaces;
using GestionInventarios.Api.Exceptions;
using GestionInventarios.Api.Models.Entities;
using GestionInventarios.Api.Util;

namespace GestionInventarios.Api.DAO.Implementations
{
    /// <summary>Implementación DAO para Marca (ADO.NET).</summary>
    public class MarcaDAO : IMarcaDAO
    {
        private const int ERR_UNIQUE_PK     = 2627;
        private const int ERR_UNIQUE_INDEX  = 2601;
        private const int ERR_FK_CONSTRAINT = 547;

        private readonly ConexionDB _conexion;

        public MarcaDAO(ConexionDB conexion) => _conexion = conexion;

        public List<Marca> Listar()
        {
            var lista = new List<Marca>();
            const string sql = @"SELECT id_marca, nombre, activo FROM marcas ORDER BY nombre;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                using SqlDataReader r    = cmd.ExecuteReader();
                while (r.Read()) lista.Add(Mapear(r));
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al consultar el listado de marcas.", ex);
            }
            return lista;
        }

        public Marca? ObtenerPorId(int idMarca)
        {
            const string sql = @"SELECT id_marca, nombre, activo FROM marcas WHERE id_marca = @id;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idMarca;
                using SqlDataReader r = cmd.ExecuteReader();
                return r.Read() ? Mapear(r) : null;
            }
            catch (SqlException ex)
            {
                throw new DAOException($"Error al consultar la marca con id {idMarca}.", ex);
            }
        }

        public int Crear(Marca m)
        {
            const string sql = @"
                INSERT INTO marcas (nombre, activo)
                OUTPUT INSERTED.id_marca
                VALUES (@nombre, @activo);";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 80).Value = m.Nombre;
                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value         = m.Activo;
                object? result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_UNIQUE_PK || ex.Number == ERR_UNIQUE_INDEX)
            {
                throw new DAOException("Ya existe una marca con ese nombre.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al crear la marca.", ex);
            }
        }

        public bool Actualizar(Marca m)
        {
            const string sql = @"UPDATE marcas SET nombre = @nombre, activo = @activo WHERE id_marca = @id;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 80).Value = m.Nombre;
                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value         = m.Activo;
                cmd.Parameters.Add("@id",     SqlDbType.Int).Value         = m.IdMarca;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_UNIQUE_PK || ex.Number == ERR_UNIQUE_INDEX)
            {
                throw new DAOException("Ya existe otra marca con ese nombre.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException($"Error al actualizar la marca con id {m.IdMarca}.", ex);
            }
        }

        public bool Eliminar(int idMarca)
        {
            const string sql = @"DELETE FROM marcas WHERE id_marca = @id;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idMarca;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_FK_CONSTRAINT)
            {
                throw new DAOException(
                    "No se puede eliminar la marca porque tiene inventarios asociados.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException($"Error al eliminar la marca con id {idMarca}.", ex);
            }
        }

        private static Marca Mapear(SqlDataReader r) => new()
        {
            IdMarca = r.GetInt32  (r.GetOrdinal("id_marca")),
            Nombre  = r.GetString (r.GetOrdinal("nombre")),
            Activo  = r.GetBoolean(r.GetOrdinal("activo"))
        };
    }
}

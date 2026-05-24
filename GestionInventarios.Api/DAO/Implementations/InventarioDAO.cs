using System.Data;
using Microsoft.Data.SqlClient;
using GestionInventarios.Api.DAO.Interfaces;
using GestionInventarios.Api.Exceptions;
using GestionInventarios.Api.Models.Entities;
using GestionInventarios.Api.Util;

namespace GestionInventarios.Api.DAO.Implementations
{
    /// <summary>Implementación DAO para Inventario (ADO.NET).</summary>
    public class InventarioDAO : IInventarioDAO
    {
        private const int ERR_UNIQUE_PK     = 2627;
        private const int ERR_UNIQUE_INDEX  = 2601;
        private const int ERR_FK_CONSTRAINT = 547;

        private readonly ConexionDB _conexion;

        public InventarioDAO(ConexionDB conexion) => _conexion = conexion;

        public List<Inventario> Listar()
        {
            var lista = new List<Inventario>();
            const string sql = @"
                SELECT  i.id_inventario, i.serial, i.id_marca, i.id_tipo, i.id_estado,
                        i.descripcion, i.fecha_ingreso, i.ubicacion, i.activo,
                        m.nombre AS marca_nombre,
                        t.nombre AS tipo_nombre,
                        e.nombre AS estado_nombre
                FROM    inventarios     i
                JOIN    marcas          m ON i.id_marca  = m.id_marca
                JOIN    tipos_equipos   t ON i.id_tipo   = t.id_tipo
                JOIN    estados_equipos e ON i.id_estado = e.id_estado
                ORDER BY i.id_inventario;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                using SqlDataReader r    = cmd.ExecuteReader();
                while (r.Read()) lista.Add(Mapear(r));
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al consultar el listado de inventarios.", ex);
            }
            return lista;
        }

        public Inventario? ObtenerPorId(int idInventario)
        {
            const string sql = @"
                SELECT  i.id_inventario, i.serial, i.id_marca, i.id_tipo, i.id_estado,
                        i.descripcion, i.fecha_ingreso, i.ubicacion, i.activo,
                        m.nombre AS marca_nombre,
                        t.nombre AS tipo_nombre,
                        e.nombre AS estado_nombre
                FROM    inventarios     i
                JOIN    marcas          m ON i.id_marca  = m.id_marca
                JOIN    tipos_equipos   t ON i.id_tipo   = t.id_tipo
                JOIN    estados_equipos e ON i.id_estado = e.id_estado
                WHERE   i.id_inventario = @id;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idInventario;
                using SqlDataReader r = cmd.ExecuteReader();
                return r.Read() ? Mapear(r) : null;
            }
            catch (SqlException ex)
            {
                throw new DAOException(
                    $"Error al consultar el inventario con id {idInventario}.", ex);
            }
        }

        public int Crear(Inventario i)
        {
            const string sql = @"
                INSERT INTO inventarios
                    (serial, id_marca, id_tipo, id_estado, descripcion, fecha_ingreso, ubicacion, activo)
                OUTPUT INSERTED.id_inventario
                VALUES (@serial, @idMarca, @idTipo, @idEstado, @descripcion, @fechaIng, @ubicacion, @activo);";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                AgregarParametros(cmd, i);
                object? result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_UNIQUE_PK || ex.Number == ERR_UNIQUE_INDEX)
            {
                throw new DAOException("Ya existe un inventario con ese serial.", ex);
            }
            catch (SqlException ex) when (ex.Number == ERR_FK_CONSTRAINT)
            {
                throw new DAOException(
                    "La marca, el tipo o el estado seleccionados no son válidos.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al crear el inventario.", ex);
            }
        }

        public bool Actualizar(Inventario i)
        {
            const string sql = @"
                UPDATE inventarios SET
                    serial        = @serial,
                    id_marca      = @idMarca,
                    id_tipo       = @idTipo,
                    id_estado     = @idEstado,
                    descripcion   = @descripcion,
                    fecha_ingreso = @fechaIng,
                    ubicacion     = @ubicacion,
                    activo        = @activo
                WHERE id_inventario = @id;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                AgregarParametros(cmd, i);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = i.IdInventario;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_UNIQUE_PK || ex.Number == ERR_UNIQUE_INDEX)
            {
                throw new DAOException("Ya existe otro inventario con ese serial.", ex);
            }
            catch (SqlException ex) when (ex.Number == ERR_FK_CONSTRAINT)
            {
                throw new DAOException(
                    "La marca, el tipo o el estado seleccionados no son válidos.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException(
                    $"Error al actualizar el inventario con id {i.IdInventario}.", ex);
            }
        }

        public bool Eliminar(int idInventario)
        {
            const string sql = @"DELETE FROM inventarios WHERE id_inventario = @id;";
            try
            {
                using SqlConnection conn = _conexion.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idInventario;
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_FK_CONSTRAINT)
            {
                throw new DAOException(
                    "No se puede eliminar el inventario porque tiene registros asociados.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException(
                    $"Error al eliminar el inventario con id {idInventario}.", ex);
            }
        }

        private static void AgregarParametros(SqlCommand cmd, Inventario i)
        {
            cmd.Parameters.Add("@serial",      SqlDbType.VarChar, 80).Value  = i.Serial;
            cmd.Parameters.Add("@idMarca",     SqlDbType.Int).Value          = i.IdMarca;
            cmd.Parameters.Add("@idTipo",      SqlDbType.Int).Value          = i.IdTipo;
            cmd.Parameters.Add("@idEstado",    SqlDbType.Int).Value          = i.IdEstado;
            cmd.Parameters.Add("@descripcion", SqlDbType.VarChar, 255).Value = (object?)i.Descripcion ?? DBNull.Value;
            cmd.Parameters.Add("@fechaIng",    SqlDbType.Date).Value         = i.FechaIngreso;
            cmd.Parameters.Add("@ubicacion",   SqlDbType.VarChar, 150).Value = (object?)i.Ubicacion ?? DBNull.Value;
            cmd.Parameters.Add("@activo",      SqlDbType.Bit).Value          = i.Activo;
        }

        private static Inventario Mapear(SqlDataReader r) => new()
        {
            IdInventario = r.GetInt32   (r.GetOrdinal("id_inventario")),
            Serial       = r.GetString  (r.GetOrdinal("serial")),
            IdMarca      = r.GetInt32   (r.GetOrdinal("id_marca")),
            IdTipo       = r.GetInt32   (r.GetOrdinal("id_tipo")),
            IdEstado     = r.GetInt32   (r.GetOrdinal("id_estado")),
            Descripcion  = r.IsDBNull(r.GetOrdinal("descripcion")) ? null : r.GetString(r.GetOrdinal("descripcion")),
            FechaIngreso = r.GetDateTime(r.GetOrdinal("fecha_ingreso")),
            Ubicacion    = r.IsDBNull(r.GetOrdinal("ubicacion"))   ? null : r.GetString(r.GetOrdinal("ubicacion")),
            Activo       = r.GetBoolean (r.GetOrdinal("activo")),
            MarcaNombre  = r.GetString  (r.GetOrdinal("marca_nombre")),
            TipoNombre   = r.GetString  (r.GetOrdinal("tipo_nombre")),
            EstadoNombre = r.GetString  (r.GetOrdinal("estado_nombre"))
        };
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using GestionFuncionarios.Exceptions;
using GestionFuncionarios.Models;
using GestionFuncionarios.Util;

namespace GestionFuncionarios.DAO
{
    /// <summary>
    /// Implementación del DAO de Funcionario sobre SQL Server usando
    /// ADO.NET (Microsoft.Data.SqlClient). Toda excepción técnica es
    /// envuelta en DAOException para no exponer detalles internos.
    /// </summary>
    public class FuncionarioDAO : IFuncionarioDAO
    {
        // Códigos de error de SQL Server comunes:
        //   2627 = violación de PRIMARY KEY
        //   2601 = violación de UNIQUE INDEX
        //   547  = violación de FOREIGN KEY / CHECK
        private const int ERR_UNIQUE_PK     = 2627;
        private const int ERR_UNIQUE_INDEX  = 2601;
        private const int ERR_FK_CONSTRAINT = 547;

        public List<Funcionario> Listar()
        {
            var lista = new List<Funcionario>();

            const string sql = @"
                SELECT  f.id_funcionario, f.id_tipo_documento, f.numero_documento,
                        f.nombres, f.apellidos, f.email, f.telefono, f.direccion,
                        f.fecha_nacimiento, f.fecha_ingreso,
                        f.id_cargo, f.id_dependencia, f.salario, f.activo,
                        td.descripcion AS tipo_documento_desc,
                        c.nombre       AS cargo_nombre,
                        d.nombre       AS dependencia_nombre
                FROM    funcionarios     f
                JOIN    tipos_documento  td ON f.id_tipo_documento = td.id_tipo_documento
                JOIN    cargos           c  ON f.id_cargo          = c.id_cargo
                JOIN    dependencias     d  ON f.id_dependencia    = d.id_dependencia
                ORDER BY f.apellidos, f.nombres;";

            try
            {
                using SqlConnection conn = ConexionDB.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                using SqlDataReader r    = cmd.ExecuteReader();

                while (r.Read())
                {
                    lista.Add(Mapear(r));
                }
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al consultar el listado de funcionarios.", ex);
            }

            return lista;
        }

        public Funcionario? ObtenerPorId(int idFuncionario)
        {
            const string sql = @"
                SELECT  f.id_funcionario, f.id_tipo_documento, f.numero_documento,
                        f.nombres, f.apellidos, f.email, f.telefono, f.direccion,
                        f.fecha_nacimiento, f.fecha_ingreso,
                        f.id_cargo, f.id_dependencia, f.salario, f.activo,
                        td.descripcion AS tipo_documento_desc,
                        c.nombre       AS cargo_nombre,
                        d.nombre       AS dependencia_nombre
                FROM    funcionarios     f
                JOIN    tipos_documento  td ON f.id_tipo_documento = td.id_tipo_documento
                JOIN    cargos           c  ON f.id_cargo          = c.id_cargo
                JOIN    dependencias     d  ON f.id_dependencia    = d.id_dependencia
                WHERE   f.id_funcionario = @id;";

            try
            {
                using SqlConnection conn = ConexionDB.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idFuncionario;

                using SqlDataReader r = cmd.ExecuteReader();
                return r.Read() ? Mapear(r) : null;
            }
            catch (SqlException ex)
            {
                throw new DAOException(
                    $"Error al consultar el funcionario con id {idFuncionario}.", ex);
            }
        }

        public int Crear(Funcionario f)
        {
            const string sql = @"
                INSERT INTO funcionarios
                    (id_tipo_documento, numero_documento, nombres, apellidos,
                     email, telefono, direccion, fecha_nacimiento, fecha_ingreso,
                     id_cargo, id_dependencia, salario, activo)
                OUTPUT INSERTED.id_funcionario
                VALUES (@idTipoDoc, @numDoc, @nombres, @apellidos,
                        @email, @telefono, @direccion, @fechaNac, @fechaIng,
                        @idCargo, @idDep, @salario, @activo);";

            try
            {
                using SqlConnection conn = ConexionDB.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                AgregarParametros(cmd, f);

                object? result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_UNIQUE_PK || ex.Number == ERR_UNIQUE_INDEX)
            {
                throw new DAOException(
                    "Ya existe un funcionario con el mismo número de documento o correo electrónico.", ex);
            }
            catch (SqlException ex) when (ex.Number == ERR_FK_CONSTRAINT)
            {
                throw new DAOException(
                    "El tipo de documento, cargo o dependencia seleccionados no son válidos.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al crear el funcionario.", ex);
            }
        }

        public bool Actualizar(Funcionario f)
        {
            const string sql = @"
                UPDATE funcionarios SET
                    id_tipo_documento = @idTipoDoc,
                    numero_documento  = @numDoc,
                    nombres           = @nombres,
                    apellidos         = @apellidos,
                    email             = @email,
                    telefono          = @telefono,
                    direccion         = @direccion,
                    fecha_nacimiento  = @fechaNac,
                    fecha_ingreso     = @fechaIng,
                    id_cargo          = @idCargo,
                    id_dependencia    = @idDep,
                    salario           = @salario,
                    activo            = @activo
                WHERE id_funcionario = @id;";

            try
            {
                using SqlConnection conn = ConexionDB.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                AgregarParametros(cmd, f);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = f.IdFuncionario;

                int filas = cmd.ExecuteNonQuery();
                return filas > 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_UNIQUE_PK || ex.Number == ERR_UNIQUE_INDEX)
            {
                throw new DAOException(
                    "Ya existe otro funcionario con el mismo número de documento o correo electrónico.", ex);
            }
            catch (SqlException ex) when (ex.Number == ERR_FK_CONSTRAINT)
            {
                throw new DAOException(
                    "El tipo de documento, cargo o dependencia seleccionados no son válidos.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException(
                    $"Error al actualizar el funcionario con id {f.IdFuncionario}.", ex);
            }
        }

        public bool Eliminar(int idFuncionario)
        {
            const string sql = @"DELETE FROM funcionarios WHERE id_funcionario = @id;";

            try
            {
                using SqlConnection conn = ConexionDB.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idFuncionario;

                int filas = cmd.ExecuteNonQuery();
                return filas > 0;
            }
            catch (SqlException ex) when (ex.Number == ERR_FK_CONSTRAINT)
            {
                // Útil si en el futuro se agregan tablas que referencien a funcionarios.
                throw new DAOException(
                    "No se puede eliminar el funcionario porque tiene registros asociados en otras tablas.", ex);
            }
            catch (SqlException ex)
            {
                throw new DAOException(
                    $"Error al eliminar el funcionario con id {idFuncionario}.", ex);
            }
        }

        // ---------------- Helpers privados ----------------

        private static Funcionario Mapear(SqlDataReader r) => new()
        {
            IdFuncionario             = r.GetInt32   (r.GetOrdinal("id_funcionario")),
            IdTipoDocumento           = r.GetInt32   (r.GetOrdinal("id_tipo_documento")),
            NumeroDocumento           = r.GetString  (r.GetOrdinal("numero_documento")),
            Nombres                   = r.GetString  (r.GetOrdinal("nombres")),
            Apellidos                 = r.GetString  (r.GetOrdinal("apellidos")),
            Email                     = r.GetString  (r.GetOrdinal("email")),
            Telefono                  = r.IsDBNull(r.GetOrdinal("telefono"))  ? null : r.GetString(r.GetOrdinal("telefono")),
            Direccion                 = r.IsDBNull(r.GetOrdinal("direccion")) ? null : r.GetString(r.GetOrdinal("direccion")),
            FechaNacimiento           = r.GetDateTime(r.GetOrdinal("fecha_nacimiento")),
            FechaIngreso              = r.GetDateTime(r.GetOrdinal("fecha_ingreso")),
            IdCargo                   = r.GetInt32   (r.GetOrdinal("id_cargo")),
            IdDependencia             = r.GetInt32   (r.GetOrdinal("id_dependencia")),
            Salario                   = r.GetDecimal (r.GetOrdinal("salario")),
            Activo                    = r.GetBoolean (r.GetOrdinal("activo")),
            TipoDocumentoDescripcion  = r.GetString  (r.GetOrdinal("tipo_documento_desc")),
            CargoNombre               = r.GetString  (r.GetOrdinal("cargo_nombre")),
            DependenciaNombre         = r.GetString  (r.GetOrdinal("dependencia_nombre"))
        };

        private static void AgregarParametros(SqlCommand cmd, Funcionario f)
        {
            cmd.Parameters.Add("@idTipoDoc", SqlDbType.Int).Value           = f.IdTipoDocumento;
            cmd.Parameters.Add("@numDoc",    SqlDbType.VarChar, 20).Value   = f.NumeroDocumento;
            cmd.Parameters.Add("@nombres",   SqlDbType.VarChar, 100).Value  = f.Nombres;
            cmd.Parameters.Add("@apellidos", SqlDbType.VarChar, 100).Value  = f.Apellidos;
            cmd.Parameters.Add("@email",     SqlDbType.VarChar, 100).Value  = f.Email;
            cmd.Parameters.Add("@telefono",  SqlDbType.VarChar, 20).Value   = (object?)f.Telefono  ?? DBNull.Value;
            cmd.Parameters.Add("@direccion", SqlDbType.VarChar, 200).Value  = (object?)f.Direccion ?? DBNull.Value;
            cmd.Parameters.Add("@fechaNac",  SqlDbType.Date).Value          = f.FechaNacimiento;
            cmd.Parameters.Add("@fechaIng",  SqlDbType.Date).Value          = f.FechaIngreso;
            cmd.Parameters.Add("@idCargo",   SqlDbType.Int).Value           = f.IdCargo;
            cmd.Parameters.Add("@idDep",     SqlDbType.Int).Value           = f.IdDependencia;
            cmd.Parameters.Add("@salario",   SqlDbType.Decimal).Value       = f.Salario;
            cmd.Parameters.Add("@activo",    SqlDbType.Bit).Value            = f.Activo;
        }
    }
}

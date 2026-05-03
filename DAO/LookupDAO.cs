using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using GestionFuncionarios.Exceptions;
using GestionFuncionarios.Models;
using GestionFuncionarios.Util;

namespace GestionFuncionarios.DAO
{
    /// <summary>
    /// DAO de sólo lectura para las tablas relacionadas con funcionarios
    /// (tipos de documento, cargos, dependencias). Se usa para poblar
    /// los ComboBox del formulario de edición. El CRUD completo, según
    /// el alcance del enunciado, sólo aplica a la tabla 'funcionarios'.
    /// </summary>
    public class LookupDAO
    {
        public List<TipoDocumento> ListarTiposDocumento()
        {
            var lista = new List<TipoDocumento>();
            const string sql = @"SELECT id_tipo_documento, codigo, descripcion
                                 FROM tipos_documento
                                 ORDER BY descripcion;";
            try
            {
                using SqlConnection conn = ConexionDB.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                using SqlDataReader r    = cmd.ExecuteReader();

                while (r.Read())
                {
                    lista.Add(new TipoDocumento
                    {
                        IdTipoDocumento = r.GetInt32(0),
                        Codigo          = r.GetString(1),
                        Descripcion     = r.GetString(2)
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al consultar los tipos de documento.", ex);
            }
            return lista;
        }

        public List<Cargo> ListarCargos()
        {
            var lista = new List<Cargo>();
            const string sql = @"SELECT id_cargo, nombre, descripcion, salario_base
                                 FROM cargos
                                 WHERE activo = 1
                                 ORDER BY nombre;";
            try
            {
                using SqlConnection conn = ConexionDB.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                using SqlDataReader r    = cmd.ExecuteReader();

                while (r.Read())
                {
                    lista.Add(new Cargo
                    {
                        IdCargo     = r.GetInt32(0),
                        Nombre      = r.GetString(1),
                        Descripcion = r.IsDBNull(2) ? null : r.GetString(2),
                        SalarioBase = r.GetDecimal(3)
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al consultar los cargos.", ex);
            }
            return lista;
        }

        public List<Dependencia> ListarDependencias()
        {
            var lista = new List<Dependencia>();
            const string sql = @"SELECT id_dependencia, nombre, descripcion
                                 FROM dependencias
                                 WHERE activo = 1
                                 ORDER BY nombre;";
            try
            {
                using SqlConnection conn = ConexionDB.ObtenerConexion();
                using SqlCommand    cmd  = new(sql, conn);
                using SqlDataReader r    = cmd.ExecuteReader();

                while (r.Read())
                {
                    lista.Add(new Dependencia
                    {
                        IdDependencia = r.GetInt32(0),
                        Nombre        = r.GetString(1),
                        Descripcion   = r.IsDBNull(2) ? null : r.GetString(2)
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new DAOException("Error al consultar las dependencias.", ex);
            }
            return lista;
        }
    }
}

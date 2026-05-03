using System;
using System.Configuration;
using Microsoft.Data.SqlClient;
using GestionFuncionarios.Exceptions;

namespace GestionFuncionarios.Util
{
    /// <summary>
    /// Clase utilitaria responsable de proporcionar conexiones a la
    /// base de datos SQL Server. Centraliza la cadena de conexión y
    /// el manejo de errores de conexión.
    /// </summary>
    public static class ConexionDB
    {
        private static readonly string CadenaConexion =
            ConfigurationManager.ConnectionStrings["GestionFuncionariosDB"]?.ConnectionString
            ?? @"Server=localhost;Database=GestionFuncionarios;Integrated Security=True;TrustServerCertificate=True;";

        /// <summary>
        /// Crea y abre una conexión a SQL Server. Lanza DAOException
        /// si no se logra establecer la conexión.
        /// </summary>
        public static SqlConnection ObtenerConexion()
        {
            try
            {
                var conexion = new SqlConnection(CadenaConexion);
                conexion.Open();
                return conexion;
            }
            catch (SqlException ex)
            {
                throw new DAOException(
                    "No fue posible conectar a la base de datos. " +
                    "Verifique que el servicio de SQL Server esté en ejecución y " +
                    "que la cadena de conexión sea correcta.",
                    ex);
            }
        }

        /// <summary>
        /// Permite probar la conexión sin lanzar excepciones.
        /// Útil al iniciar la aplicación.
        /// </summary>
        public static bool ProbarConexion(out string mensaje)
        {
            try
            {
                using var conn = ObtenerConexion();
                mensaje = "Conexión exitosa.";
                return true;
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }
    }
}

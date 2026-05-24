using Microsoft.Data.SqlClient;
using GestionInventarios.Api.Exceptions;

namespace GestionInventarios.Api.Util
{
    /// <summary>
    /// Clase utilitaria responsable de proporcionar conexiones a la
    /// base de datos SQL Server. Centraliza la cadena de conexión
    /// (leída desde IConfiguration -> ConnectionStrings:Default) y el
    /// manejo de errores de conexión envolviéndolos en DAOException.
    /// </summary>
    public class ConexionDB
    {
        private readonly string _cadenaConexion;

        public ConexionDB(IConfiguration configuration)
        {
            _cadenaConexion =
                configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'Default' en appsettings.json.");
        }

        /// <summary>
        /// Crea y abre una conexión a SQL Server. Lanza DAOException si
        /// no se logra establecer la conexión.
        /// </summary>
        public SqlConnection ObtenerConexion()
        {
            try
            {
                var conexion = new SqlConnection(_cadenaConexion);
                conexion.Open();
                return conexion;
            }
            catch (SqlException ex)
            {
                throw new DAOException(
                    "No fue posible conectar a la base de datos. " +
                    "Verifique que el servicio de SQL Server esté en ejecución y " +
                    "que la cadena de conexión sea correcta.", ex);
            }
        }

        /// <summary>
        /// Prueba la conexión sin lanzar excepciones. Útil al iniciar
        /// la aplicación.
        /// </summary>
        public bool ProbarConexion(out string mensaje)
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

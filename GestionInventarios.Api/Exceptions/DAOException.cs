namespace GestionInventarios.Api.Exceptions
{
    /// <summary>
    /// Excepción personalizada para errores ocurridos en la capa de
    /// acceso a datos (DAO). Envuelve excepciones técnicas
    /// (SqlException, IOException, etc.) y las propaga hacia las capas
    /// superiores con un mensaje significativo, sin exponer detalles
    /// internos de la base de datos.
    /// </summary>
    public class DAOException : Exception
    {
        public DAOException() : base() { }

        public DAOException(string mensaje) : base(mensaje) { }

        public DAOException(string mensaje, Exception innerException)
            : base(mensaje, innerException) { }
    }
}

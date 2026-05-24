namespace GestionInventarios.Api.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando una operación de autenticación falla:
    /// credenciales inválidas, usuario inactivo, etc. Se traduce a un
    /// HTTP 401 desde el middleware de manejo de excepciones.
    /// </summary>
    public class AuthenticationException : Exception
    {
        public AuthenticationException() : base() { }

        public AuthenticationException(string mensaje) : base(mensaje) { }

        public AuthenticationException(string mensaje, Exception innerException)
            : base(mensaje, innerException) { }
    }
}

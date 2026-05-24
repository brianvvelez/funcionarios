namespace GestionInventarios.Api.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando una entidad solicitada por id no existe.
    /// Se traduce a un HTTP 404 desde el middleware de manejo de
    /// excepciones.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException() : base() { }

        public NotFoundException(string mensaje) : base(mensaje) { }

        public NotFoundException(string mensaje, Exception innerException)
            : base(mensaje, innerException) { }
    }
}

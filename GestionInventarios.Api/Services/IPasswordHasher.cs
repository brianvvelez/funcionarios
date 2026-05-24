namespace GestionInventarios.Api.Services
{
    /// <summary>
    /// Abstracción sobre la librería de hashing de contraseñas. Permite
    /// reemplazar BCrypt por otra implementación sin tocar las capas
    /// superiores (Services, Controllers).
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>Genera un hash seguro de la contraseña.</summary>
        string Hash(string passwordPlano);

        /// <summary>Verifica si la contraseña en texto plano coincide con el hash.</summary>
        bool Verify(string passwordPlano, string passwordHash);
    }
}

namespace GestionInventarios.Api.Services
{
    /// <summary>
    /// Implementación de IPasswordHasher basada en BCrypt.Net-Next.
    /// El work-factor 12 ofrece un balance razonable entre seguridad
    /// y latencia (~250 ms por hash en hardware moderno).
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;

        public string Hash(string passwordPlano) =>
            BCrypt.Net.BCrypt.HashPassword(passwordPlano, WorkFactor);

        public bool Verify(string passwordPlano, string passwordHash) =>
            BCrypt.Net.BCrypt.Verify(passwordPlano, passwordHash);
    }
}

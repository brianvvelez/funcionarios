using GestionInventarios.Api.Models.DTOs;

namespace GestionInventarios.Api.Services
{
    /// <summary>
    /// Servicio de autenticación: valida credenciales y emite el
    /// token JWT que se usará en los endpoints protegidos.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Valida las credenciales. Si son correctas, devuelve la
        /// respuesta con el token JWT. Si no, lanza
        /// AuthenticationException — el middleware la traduce en 401
        /// con un mensaje genérico para evitar enumeración de usuarios.
        /// </summary>
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}

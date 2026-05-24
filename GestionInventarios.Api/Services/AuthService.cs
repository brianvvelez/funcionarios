using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using GestionInventarios.Api.DAO.Interfaces;
using GestionInventarios.Api.Exceptions;
using GestionInventarios.Api.Models.DTOs;

namespace GestionInventarios.Api.Services
{
    /// <summary>
    /// Implementación de IAuthService. Verifica la contraseña contra el
    /// hash BCrypt almacenado en la base de datos y emite un JWT
    /// firmado con HMAC-SHA256 cuya clave se lee de appsettings.json.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUsuarioDAO     _usuarioDAO;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration  _configuration;

        public AuthService(
            IUsuarioDAO     usuarioDAO,
            IPasswordHasher passwordHasher,
            IConfiguration  configuration)
        {
            _usuarioDAO     = usuarioDAO;
            _passwordHasher = passwordHasher;
            _configuration  = configuration;
        }

        public Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var usuario = _usuarioDAO.ObtenerPorEmail(request.Email);

            // Mismo mensaje para los tres casos (usuario inexistente,
            // password incorrecta, usuario inactivo) para no facilitar
            // enumeración de cuentas válidas.
            if (usuario is null ||
                !usuario.Activo ||
                !_passwordHasher.Verify(request.Password, usuario.PasswordHash))
            {
                throw new AuthenticationException("Credenciales inválidas.");
            }

            var (token, expira) = GenerarToken(usuario.IdUsuario, usuario.Email, usuario.Rol);

            return Task.FromResult(new LoginResponse
            {
                Token    = token,
                Email    = usuario.Email,
                Rol      = usuario.Rol,
                ExpiraEn = expira
            });
        }

        private (string token, DateTime expira) GenerarToken(int idUsuario, string email, string rol)
        {
            var clave        = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Falta la clave 'Jwt:Key' en appsettings.");
            var issuer       = _configuration["Jwt:Issuer"]   ?? "GestionInventarios.Api";
            var audience     = _configuration["Jwt:Audience"] ?? "GestionInventarios.Clients";
            var minutos      = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var m) ? m : 60;

            if (Encoding.UTF8.GetByteCount(clave) < 32)
            {
                throw new InvalidOperationException(
                    "La clave Jwt:Key debe tener al menos 32 caracteres / 256 bits.");
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, idUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, rol),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var llave        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave));
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);
            var expira       = DateTime.UtcNow.AddMinutes(minutos);

            var jwt = new JwtSecurityToken(
                issuer:             issuer,
                audience:           audience,
                claims:             claims,
                notBefore:          DateTime.UtcNow,
                expires:            expira,
                signingCredentials: credenciales);

            return (new JwtSecurityTokenHandler().WriteToken(jwt), expira);
        }
    }
}

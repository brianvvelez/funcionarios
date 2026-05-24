using Microsoft.AspNetCore.Mvc;
using GestionInventarios.Api.Models.DTOs;
using GestionInventarios.Api.Services;

namespace GestionInventarios.Api.Controllers
{
    /// <summary>Endpoints de autenticación.</summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        /// <summary>
        /// Autentica un usuario y devuelve un token JWT que debe enviarse
        /// en el header Authorization como "Bearer {token}" en las
        /// siguientes peticiones a endpoints protegidos.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
    }
}

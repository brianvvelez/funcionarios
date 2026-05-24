using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionInventarios.Api.DAO.Interfaces;
using GestionInventarios.Api.Exceptions;
using GestionInventarios.Api.Models.DTOs;
using GestionInventarios.Api.Models.Entities;
using GestionInventarios.Api.Services;

namespace GestionInventarios.Api.Controllers
{
    /// <summary>CRUD de usuarios. Sólo accesible para rol Administrador.</summary>
    [ApiController]
    [Route("api/usuarios")]
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioDAO     _usuarioDAO;
        private readonly IPasswordHasher _passwordHasher;

        public UsuariosController(IUsuarioDAO usuarioDAO, IPasswordHasher passwordHasher)
        {
            _usuarioDAO     = usuarioDAO;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UsuarioResponseDto>> Listar()
        {
            var lista = _usuarioDAO.Listar().Select(ToDto);
            return Ok(lista);
        }

        [HttpGet("{id:int}")]
        public ActionResult<UsuarioResponseDto> ObtenerPorId(int id)
        {
            var u = _usuarioDAO.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe el usuario con id {id}.");
            return Ok(ToDto(u));
        }

        [HttpPost]
        public ActionResult<UsuarioResponseDto> Crear([FromBody] UsuarioCreateDto dto)
        {
            var nuevo = new Usuario
            {
                Nombres      = dto.Nombres.Trim(),
                Apellidos    = dto.Apellidos.Trim(),
                Email        = dto.Email.Trim().ToLowerInvariant(),
                PasswordHash = _passwordHasher.Hash(dto.Password),
                Rol          = dto.Rol,
                Activo       = true
            };

            nuevo.IdUsuario = _usuarioDAO.Crear(nuevo);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.IdUsuario }, ToDto(nuevo));
        }

        [HttpPut("{id:int}")]
        public ActionResult<UsuarioResponseDto> Actualizar(int id, [FromBody] UsuarioUpdateDto dto)
        {
            var actual = _usuarioDAO.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe el usuario con id {id}.");

            actual.Nombres   = dto.Nombres.Trim();
            actual.Apellidos = dto.Apellidos.Trim();
            actual.Email     = dto.Email.Trim().ToLowerInvariant();
            actual.Rol       = dto.Rol;
            actual.Activo    = dto.Activo;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                actual.PasswordHash = _passwordHasher.Hash(dto.Password);
            }

            _usuarioDAO.Actualizar(actual);
            return Ok(ToDto(actual));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Eliminar(int id)
        {
            var existe = _usuarioDAO.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe el usuario con id {id}.");
            _usuarioDAO.Eliminar(existe.IdUsuario);
            return NoContent();
        }

        private static UsuarioResponseDto ToDto(Usuario u) => new()
        {
            IdUsuario     = u.IdUsuario,
            Nombres       = u.Nombres,
            Apellidos     = u.Apellidos,
            Email         = u.Email,
            Rol           = u.Rol,
            FechaCreacion = u.FechaCreacion,
            Activo        = u.Activo
        };
    }
}

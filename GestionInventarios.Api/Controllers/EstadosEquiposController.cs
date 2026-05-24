using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionInventarios.Api.DAO.Interfaces;
using GestionInventarios.Api.Exceptions;
using GestionInventarios.Api.Models.DTOs;
using GestionInventarios.Api.Models.Entities;

namespace GestionInventarios.Api.Controllers
{
    /// <summary>CRUD de estados de equipos. Sólo Administrador.</summary>
    [ApiController]
    [Route("api/estados-equipos")]
    [Authorize(Roles = "Administrador")]
    public class EstadosEquiposController : ControllerBase
    {
        private readonly IEstadoEquipoDAO _dao;

        public EstadosEquiposController(IEstadoEquipoDAO dao) => _dao = dao;

        [HttpGet]
        public ActionResult<IEnumerable<EstadoEquipoResponseDto>> Listar() =>
            Ok(_dao.Listar().Select(ToDto));

        [HttpGet("{id:int}")]
        public ActionResult<EstadoEquipoResponseDto> ObtenerPorId(int id)
        {
            var e = _dao.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe el estado con id {id}.");
            return Ok(ToDto(e));
        }

        [HttpPost]
        public ActionResult<EstadoEquipoResponseDto> Crear([FromBody] EstadoEquipoCreateDto dto)
        {
            var nuevo = new EstadoEquipo
            {
                Nombre      = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion?.Trim(),
                Activo      = dto.Activo
            };
            nuevo.IdEstado = _dao.Crear(nuevo);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.IdEstado }, ToDto(nuevo));
        }

        [HttpPut("{id:int}")]
        public ActionResult<EstadoEquipoResponseDto> Actualizar(int id, [FromBody] EstadoEquipoCreateDto dto)
        {
            var actual = _dao.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe el estado con id {id}.");

            actual.Nombre      = dto.Nombre.Trim();
            actual.Descripcion = dto.Descripcion?.Trim();
            actual.Activo      = dto.Activo;

            _dao.Actualizar(actual);
            return Ok(ToDto(actual));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Eliminar(int id)
        {
            var existe = _dao.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe el estado con id {id}.");
            _dao.Eliminar(existe.IdEstado);
            return NoContent();
        }

        private static EstadoEquipoResponseDto ToDto(EstadoEquipo e) => new()
        {
            IdEstado    = e.IdEstado,
            Nombre      = e.Nombre,
            Descripcion = e.Descripcion,
            Activo      = e.Activo
        };
    }
}

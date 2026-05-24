using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionInventarios.Api.DAO.Interfaces;
using GestionInventarios.Api.Exceptions;
using GestionInventarios.Api.Models.DTOs;
using GestionInventarios.Api.Models.Entities;

namespace GestionInventarios.Api.Controllers
{
    /// <summary>CRUD de tipos de equipos. Sólo Administrador.</summary>
    [ApiController]
    [Route("api/tipos-equipos")]
    [Authorize(Roles = "Administrador")]
    public class TiposEquiposController : ControllerBase
    {
        private readonly ITipoEquipoDAO _dao;

        public TiposEquiposController(ITipoEquipoDAO dao) => _dao = dao;

        [HttpGet]
        public ActionResult<IEnumerable<TipoEquipoResponseDto>> Listar() =>
            Ok(_dao.Listar().Select(ToDto));

        [HttpGet("{id:int}")]
        public ActionResult<TipoEquipoResponseDto> ObtenerPorId(int id)
        {
            var t = _dao.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe el tipo con id {id}.");
            return Ok(ToDto(t));
        }

        [HttpPost]
        public ActionResult<TipoEquipoResponseDto> Crear([FromBody] TipoEquipoCreateDto dto)
        {
            var nuevo = new TipoEquipo
            {
                Nombre      = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion?.Trim(),
                Activo      = dto.Activo
            };
            nuevo.IdTipo = _dao.Crear(nuevo);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.IdTipo }, ToDto(nuevo));
        }

        [HttpPut("{id:int}")]
        public ActionResult<TipoEquipoResponseDto> Actualizar(int id, [FromBody] TipoEquipoCreateDto dto)
        {
            var actual = _dao.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe el tipo con id {id}.");

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
                ?? throw new NotFoundException($"No existe el tipo con id {id}.");
            _dao.Eliminar(existe.IdTipo);
            return NoContent();
        }

        private static TipoEquipoResponseDto ToDto(TipoEquipo t) => new()
        {
            IdTipo      = t.IdTipo,
            Nombre      = t.Nombre,
            Descripcion = t.Descripcion,
            Activo      = t.Activo
        };
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionInventarios.Api.DAO.Interfaces;
using GestionInventarios.Api.Exceptions;
using GestionInventarios.Api.Models.DTOs;
using GestionInventarios.Api.Models.Entities;

namespace GestionInventarios.Api.Controllers
{
    /// <summary>CRUD de marcas. Sólo Administrador.</summary>
    [ApiController]
    [Route("api/marcas")]
    [Authorize(Roles = "Administrador")]
    public class MarcasController : ControllerBase
    {
        private readonly IMarcaDAO _dao;

        public MarcasController(IMarcaDAO dao) => _dao = dao;

        [HttpGet]
        public ActionResult<IEnumerable<MarcaResponseDto>> Listar() =>
            Ok(_dao.Listar().Select(ToDto));

        [HttpGet("{id:int}")]
        public ActionResult<MarcaResponseDto> ObtenerPorId(int id)
        {
            var m = _dao.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe la marca con id {id}.");
            return Ok(ToDto(m));
        }

        [HttpPost]
        public ActionResult<MarcaResponseDto> Crear([FromBody] MarcaCreateDto dto)
        {
            var nueva = new Marca { Nombre = dto.Nombre.Trim(), Activo = dto.Activo };
            nueva.IdMarca = _dao.Crear(nueva);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = nueva.IdMarca }, ToDto(nueva));
        }

        [HttpPut("{id:int}")]
        public ActionResult<MarcaResponseDto> Actualizar(int id, [FromBody] MarcaCreateDto dto)
        {
            var actual = _dao.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe la marca con id {id}.");
            actual.Nombre = dto.Nombre.Trim();
            actual.Activo = dto.Activo;
            _dao.Actualizar(actual);
            return Ok(ToDto(actual));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Eliminar(int id)
        {
            var existe = _dao.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe la marca con id {id}.");
            _dao.Eliminar(existe.IdMarca);
            return NoContent();
        }

        private static MarcaResponseDto ToDto(Marca m) => new()
        {
            IdMarca = m.IdMarca,
            Nombre  = m.Nombre,
            Activo  = m.Activo
        };
    }
}

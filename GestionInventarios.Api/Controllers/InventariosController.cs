using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionInventarios.Api.DAO.Interfaces;
using GestionInventarios.Api.Exceptions;
using GestionInventarios.Api.Models.DTOs;
using GestionInventarios.Api.Models.Entities;

namespace GestionInventarios.Api.Controllers
{
    /// <summary>
    /// CRUD de inventarios.
    /// - GET (listar / obtener por id): cualquier usuario autenticado
    ///   (Administrador o Docente).
    /// - POST / PUT / DELETE: sólo Administrador.
    /// </summary>
    [ApiController]
    [Route("api/inventarios")]
    [Authorize]
    public class InventariosController : ControllerBase
    {
        private readonly IInventarioDAO _inventarioDAO;

        public InventariosController(IInventarioDAO inventarioDAO) =>
            _inventarioDAO = inventarioDAO;

        [HttpGet]
        [Authorize(Roles = "Administrador,Docente")]
        public ActionResult<IEnumerable<InventarioResponseDto>> Listar()
        {
            var lista = _inventarioDAO.Listar().Select(ToDto);
            return Ok(lista);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Administrador,Docente")]
        public ActionResult<InventarioResponseDto> ObtenerPorId(int id)
        {
            var inv = _inventarioDAO.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe el inventario con id {id}.");
            return Ok(ToDto(inv));
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public ActionResult<InventarioResponseDto> Crear([FromBody] InventarioCreateDto dto)
        {
            var nuevo = new Inventario
            {
                Serial       = dto.Serial.Trim(),
                IdMarca      = dto.IdMarca,
                IdTipo       = dto.IdTipo,
                IdEstado     = dto.IdEstado,
                Descripcion  = dto.Descripcion?.Trim(),
                FechaIngreso = dto.FechaIngreso,
                Ubicacion    = dto.Ubicacion?.Trim(),
                Activo       = dto.Activo
            };

            nuevo.IdInventario = _inventarioDAO.Crear(nuevo);

            // Re-leer para que la respuesta traiga los nombres de las relaciones.
            var creado = _inventarioDAO.ObtenerPorId(nuevo.IdInventario)!;
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.IdInventario }, ToDto(creado));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public ActionResult<InventarioResponseDto> Actualizar(int id, [FromBody] InventarioCreateDto dto)
        {
            var actual = _inventarioDAO.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe el inventario con id {id}.");

            actual.Serial       = dto.Serial.Trim();
            actual.IdMarca      = dto.IdMarca;
            actual.IdTipo       = dto.IdTipo;
            actual.IdEstado     = dto.IdEstado;
            actual.Descripcion  = dto.Descripcion?.Trim();
            actual.FechaIngreso = dto.FechaIngreso;
            actual.Ubicacion    = dto.Ubicacion?.Trim();
            actual.Activo       = dto.Activo;

            _inventarioDAO.Actualizar(actual);

            var actualizado = _inventarioDAO.ObtenerPorId(id)!;
            return Ok(ToDto(actualizado));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public IActionResult Eliminar(int id)
        {
            var existe = _inventarioDAO.ObtenerPorId(id)
                ?? throw new NotFoundException($"No existe el inventario con id {id}.");
            _inventarioDAO.Eliminar(existe.IdInventario);
            return NoContent();
        }

        private static InventarioResponseDto ToDto(Inventario i) => new()
        {
            IdInventario = i.IdInventario,
            Serial       = i.Serial,
            IdMarca      = i.IdMarca,
            MarcaNombre  = i.MarcaNombre,
            IdTipo       = i.IdTipo,
            TipoNombre   = i.TipoNombre,
            IdEstado     = i.IdEstado,
            EstadoNombre = i.EstadoNombre,
            Descripcion  = i.Descripcion,
            FechaIngreso = i.FechaIngreso,
            Ubicacion    = i.Ubicacion,
            Activo       = i.Activo
        };
    }
}

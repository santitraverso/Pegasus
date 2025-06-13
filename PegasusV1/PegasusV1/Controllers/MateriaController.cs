using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MateriaController : BaseSecureController<Materia>
    {
        private readonly IService<Curso> _cursoService;
        private readonly IService<ContenidoMaterias> _contenidoMateriaService;

        public MateriaController(
            IService<Materia> materiaService,
            IService<ContenidoMaterias> contenidoMateriaService,
            IService<Curso> cursoService)
            : base(materiaService)
        {
            _cursoService = cursoService;
            _contenidoMateriaService = contenidoMateriaService;
        }

        [HttpGet]
        [Route("GetMateriasForCombo")]
        public async Task<ActionResult<List<Materia>>> GetMateriasForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<Materia?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateMateria")]
        public async Task<ActionResult<Materia>> CreateMateria([FromBody] Materia materia)
        {
            try
            {
                var result = await _service.Create(materia);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateMateria")]
        public async Task<ActionResult<Materia>> UpdateMateria([FromBody] Materia materia)
        {
            try
            {
                var result = await _service.Update(materia);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteMateria/{id}")]
        public async Task<ActionResult> DeleteMateria(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var materia = await _service.GetById(id);
                if (materia != null)
                {
                    var relaciones = await _contenidoMateriaService.GetContenidoMateriasForCombo(cm => cm.Id_Materia == id);

                    // Eliminar cada relación
                    foreach (var relacion in relaciones)
                    {
                        await _contenidoMateriaService.Delete(relacion);
                    }

                    await _service.Delete(materia);
                    return Ok();
                }
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        [Route("CreateAllMateria")]
        public async Task<ActionResult<List<Materia>>> CreateAllMateria([FromBody] List<Materia> materias)
        {
            try
            {
                var result = await _service.CreateAll(materias);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllMateria")]
        public async Task<ActionResult<List<Materia>>> UpdateAllMateria([FromBody] List<Materia> materias)
        {
            try
            {
                var result = await _service.UpdateAll(materias);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllMateria")]
        public async Task<ActionResult> DeleteAllMateria([FromBody] List<Materia> materias)
        {
            try
            {
                await _service.DeleteAll(materias);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

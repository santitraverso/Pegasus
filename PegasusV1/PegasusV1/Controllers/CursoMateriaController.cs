using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CursoMateriaController : BaseSecureController<CursoMateria>
    {
        private readonly IService<Materia> _materiaService;
        private readonly IService<Curso> _cursoService;

        public CursoMateriaController(
            IService<CursoMateria> cursoMateriaService,
            IService<Materia> materiaService,
            IService<Curso> cursoService)
            : base(cursoMateriaService)
        {
            _materiaService = materiaService;
            _cursoService = cursoService;
        }

        [HttpGet]
        [Route("GetCursoMateriaForCombo")]
        public async Task<ActionResult<List<CursoMateria>>> GetCursoMateriaForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<CursoMateria>> GetForComboInternal(Expression<Func<CursoMateria, bool>>? predicate)
        {
            var cursoMaterias = await _service.GetCursoMateriaForCombo(predicate);

            foreach (var item in cursoMaterias)
            {
                if (item.Id_Curso.HasValue && item.Id_Materia.HasValue)
                {
                    item.Curso = await _cursoService.GetById(item.Id_Curso.Value);
                    item.Materia = await _materiaService.GetById(item.Id_Materia.Value);
                }
            }

            return cursoMaterias;
        }

        protected override async Task<CursoMateria?> GetByIdInternal(int id)
        {
            var cursoMateria = await _service.GetById(id);

            if (cursoMateria != null)
            {
                if (cursoMateria.Id_Materia.HasValue)
                {
                    cursoMateria.Materia = await _materiaService.GetById(cursoMateria.Id_Materia.Value);
                }

                if (cursoMateria.Id_Curso.HasValue)
                {
                    cursoMateria.Curso = await _cursoService.GetById(cursoMateria.Id_Curso.Value);
                }
            }

            return cursoMateria;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<CursoMateria?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateCursoMateria")]
        public async Task<ActionResult<CursoMateria>> CreateCursoMateria([FromBody] CursoMateria cursoMateria)
        {
            try
            {
                var result = await _service.Create(cursoMateria);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateCursoMateria")]
        public async Task<ActionResult<CursoMateria>> UpdateCursoMateria([FromBody] CursoMateria cursoMateria)
        {
            try
            {
                var result = await _service.Update(cursoMateria);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteCursoMateria/{id}")]
        public async Task<ActionResult> DeleteCursoMateria(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var cursoMateria = await _service.GetById(id);
                if (cursoMateria != null)
                {
                    await _service.Delete(cursoMateria);
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
        [Route("CreateAllCursoMateria")]
        public async Task<ActionResult<List<CursoMateria>>> CreateAllCursoMateria([FromBody] List<CursoMateria> cursoMaterias)
        {
            try
            {
                var result = await _service.CreateAll(cursoMaterias);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllCursoMateria")]
        public async Task<ActionResult<List<CursoMateria>>> UpdateAllCursoMateria([FromBody] List<CursoMateria> cursoMaterias)
        {
            try
            {
                var result = await _service.UpdateAll(cursoMaterias);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllCursoMateria")]
        public async Task<ActionResult> DeleteAllCursoMateria([FromBody] List<CursoMateria> cursoMaterias)
        {
            try
            {
                // Obtener solo los IDs de las entidades a eliminar
                var ids = cursoMaterias.Select(cm => cm.Id).ToList();

                foreach (var id in ids)
                {
                    var entity = await _service.GetById(id);
                    if (entity != null)
                    {
                        entity.Curso = null;
                        entity.Materia = null;

                        await _service.Delete(entity);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}

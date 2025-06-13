using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CursoController : BaseSecureController<Curso>
    {
        public CursoController(IService<Curso> cursoService)
            : base(cursoService)
        {
        }

        [HttpGet]
        [Route("GetCursosForCombo")]
        public async Task<ActionResult<List<Curso>>> GetCursosForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<Curso>> GetForComboInternal(Expression<Func<Curso, bool>>? predicate)
        {
            return await _service.GetCursoForCombo(predicate);
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<Curso?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateCurso")]
        public async Task<ActionResult<Curso>> CreateCurso([FromBody] Curso curso)
        {
            try
            {
                var result = await _service.Create(curso);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateCurso")]
        public async Task<ActionResult<Curso>> UpdateCurso([FromBody] Curso curso)
        {
            try
            {
                var result = await _service.Update(curso);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteCurso/{id}")]
        public async Task<ActionResult> DeleteCurso(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var curso = await _service.GetById(id);
                if (curso != null)
                {
                    await _service.Delete(curso);
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
        [Route("CreateAllCurso")]
        public async Task<ActionResult<List<Curso>>> CreateAllCurso([FromBody] List<Curso> cursos)
        {
            try
            {
                var result = await _service.CreateAll(cursos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllCurso")]
        public async Task<ActionResult<List<Curso>>> UpdateAllCurso([FromBody] List<Curso> cursos)
        {
            try
            {
                var result = await _service.UpdateAll(cursos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllCurso")]
        public async Task<ActionResult> DeleteAllCurso([FromBody] List<Curso> cursos)
        {
            try
            {
                await _service.DeleteAll(cursos);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

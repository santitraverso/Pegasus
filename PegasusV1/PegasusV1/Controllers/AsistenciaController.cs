using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Web;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AsistenciaController : BaseSecureController<Asistencia>
    {
        private readonly IService<Materia> MateriaService;
        private readonly IService<Curso> CursoService;
        private readonly IService<Usuario> UsuarioService;

        public AsistenciaController(
            IService<Asistencia> asistenciaService,
            IService<Materia> materiaService,
            IService<Curso> cursoService,
            IService<Usuario> usuarioService)
            : base(asistenciaService)
        {
            MateriaService = materiaService;
            CursoService = cursoService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetAsistenciasForCombo")]
        public async Task<ActionResult<List<Asistencia>>> GetAsistenciasForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<Asistencia>> GetForComboInternal(Expression<Func<Asistencia, bool>>? predicate)
        {
            return await _service.GetAsistenciasForCombo(predicate);
        }

        protected override async Task<Asistencia?> GetByIdInternal(int id)
        {
            var asistencia = await _service.GetById(id);

            if (asistencia != null)
            {
                if (asistencia.Id_Materia.HasValue)
                {
                    asistencia.Materia = await MateriaService.GetById(asistencia.Id_Materia.Value);
                }

                if (asistencia.Id_Alumno.HasValue)
                {
                    asistencia.Alumno = await UsuarioService.GetById(asistencia.Id_Alumno.Value);
                }

                if (asistencia.Id_Curso.HasValue)
                {
                    asistencia.Curso = await CursoService.GetById(asistencia.Id_Curso.Value);
                }
            }

            return asistencia;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<Asistencia?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateAsistencia")]
        public async Task<ActionResult<Asistencia>> CreateAsistencia([FromBody] Asistencia asistencia)
        {
            try
            {
                var result = await _service.Create(asistencia);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAsistencia")]
        public async Task<ActionResult<Asistencia>> UpdateAsistencia([FromBody] Asistencia asistencia)
        {
            try
            {
                var result = await _service.Update(asistencia);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAsistencia/{id}")]
        public async Task<ActionResult> DeleteAsistencia(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var asistencia = await _service.GetById(id);
                if (asistencia != null)
                {
                    await _service.Delete(asistencia);
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
        [Route("CreateAllAsistencia")]
        public async Task<ActionResult<List<Asistencia>>> CreateAllAsistencia([FromBody] List<Asistencia> asistencias)
        {
            try
            {
                var result = await _service.CreateAll(asistencias);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllAsistencia")]
        public async Task<ActionResult<List<Asistencia>>> UpdateAllAsistencia([FromBody] List<Asistencia> asistencias)
        {
            try
            {
                var result = await _service.UpdateAll(asistencias);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllAsistencia")]
        public async Task<ActionResult> DeleteAllAsistencia([FromBody] List<Asistencia> asistencias)
        {
            try
            {
                await _service.DeleteAll(asistencias);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

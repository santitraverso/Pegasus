using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocenteMateriaController : BaseSecureController<DocenteMateria>
    {
        private readonly IService<Materia> _materiaService;
        private readonly IService<Usuario> _usuarioService;
        private readonly IService<Curso> _cursoService;

        public DocenteMateriaController(
            IService<DocenteMateria> docenteMateriaService,
            IService<Materia> materiaService,
            IService<Usuario> usuarioService,
            IService<Curso> cursoService)
            : base(docenteMateriaService)
        {
            _materiaService = materiaService;
            _usuarioService = usuarioService;
            _cursoService = cursoService;
        }

        [HttpGet]
        [Route("GetDocenteMateriaForCombo")]
        public async Task<ActionResult<List<DocenteMateria>>> GetDocenteMateriaForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<DocenteMateria>> GetForComboInternal(Expression<Func<DocenteMateria, bool>>? predicate)
        {
            var docenteMaterias = await _service.GetDocenteMateriaForCombo(predicate);

            foreach (var item in docenteMaterias)
            {
                if (item.Id_Curso.HasValue && item.Id_Materia.HasValue && item.Id_Docente.HasValue)
                {
                    item.Curso = await _cursoService.GetById(item.Id_Curso.Value);
                    item.Materia = await _materiaService.GetById(item.Id_Materia.Value);
                    item.Docente = await _usuarioService.GetById(item.Id_Docente.Value);
                }
            }

            return docenteMaterias;
        }

        protected override async Task<DocenteMateria?> GetByIdInternal(int id)
        {
            var docenteMateria = await _service.GetById(id);

            if (docenteMateria != null)
            {
                if (docenteMateria.Id_Materia.HasValue)
                {
                    docenteMateria.Materia = await _materiaService.GetById(docenteMateria.Id_Materia.Value);
                }

                if (docenteMateria.Id_Curso.HasValue)
                {
                    docenteMateria.Curso = await _cursoService.GetById(docenteMateria.Id_Curso.Value);
                }

                if (docenteMateria.Id_Docente.HasValue)
                {
                    docenteMateria.Docente = await _usuarioService.GetById(docenteMateria.Id_Docente.Value);
                }
            }

            return docenteMateria;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<DocenteMateria?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateDocenteMateria")]
        public async Task<ActionResult<DocenteMateria>> CreateDocenteMateria([FromBody] DocenteMateria docenteMateria)
        {
            try
            {
                var result = await _service.Create(docenteMateria);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateDocenteMateria")]
        public async Task<ActionResult<DocenteMateria>> UpdateDocenteMateria([FromBody] DocenteMateria docenteMateria)
        {
            try
            {
                var result = await _service.Update(docenteMateria);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteDocenteMateria/{id}")]
        public async Task<ActionResult> DeleteDocenteMateria(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var docenteMateria = await _service.GetById(id);
                if (docenteMateria != null)
                {
                    await _service.Delete(docenteMateria);
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
        [Route("CreateAllDocenteMateria")]
        public async Task<ActionResult<List<DocenteMateria>>> CreateAllDocenteMateria([FromBody] List<DocenteMateria> docenteMaterias)
        {
            try
            {
                var result = await _service.CreateAll(docenteMaterias);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllDocenteMateria")]
        public async Task<ActionResult<List<DocenteMateria>>> UpdateAllDocenteMateria([FromBody] List<DocenteMateria> docenteMaterias)
        {
            try
            {
                var result = await _service.UpdateAll(docenteMaterias);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllDocenteMateria")]
        public async Task<ActionResult> DeleteAllDocenteMateria([FromBody] List<DocenteMateria> docenteMaterias)
        {
            try
            {
                var ids = docenteMaterias.Select(dm => dm.Id).ToList();

                foreach (var id in ids)
                {
                    var entity = await _service.GetById(id);
                    if (entity != null)
                    {
                        await _service.Delete(entity);
                    }
                }

                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

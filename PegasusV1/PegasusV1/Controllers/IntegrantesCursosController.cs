using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IntegrantesCursosController : BaseSecureController<IntegrantesCursos>
    {
        private readonly IService<Curso> _cursoService;
        private readonly IService<Usuario> _usuarioService;

        public IntegrantesCursosController(
            IService<IntegrantesCursos> integrantesCursosService,
            IService<Curso> cursoService,
            IService<Usuario> usuarioService)
            : base(integrantesCursosService)
        {
            _cursoService = cursoService;
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetIntegrantesCursosForCombo")]
        public async Task<ActionResult<List<IntegrantesCursos>>> GetIntegrantesCursosForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<IntegrantesCursos>> GetForComboInternal(Expression<Func<IntegrantesCursos, bool>>? predicate)
        {
            var integrantesCursos = await _service.GetIntegrantesCursosForCombo(predicate);

            foreach (var integrante in integrantesCursos)
            {
                if (integrante.Id_Curso.HasValue)
                {
                    integrante.Curso = await _cursoService.GetById(integrante.Id_Curso.Value);
                }

                if (integrante.Id_Usuario.HasValue)
                {
                    integrante.Usuario = await _usuarioService.GetById(integrante.Id_Usuario.Value);
                }
            }

            return integrantesCursos;
        }

        protected override async Task<IntegrantesCursos?> GetByIdInternal(int id)
        {
            var integrantesCursos = await _service.GetById(id);

            if (integrantesCursos != null)
            {
                if (integrantesCursos.Id_Curso.HasValue)
                {
                    integrantesCursos.Curso = await _cursoService.GetById(integrantesCursos.Id_Curso.Value);
                }

                if (integrantesCursos.Id_Usuario.HasValue)
                {
                    integrantesCursos.Usuario = await _usuarioService.GetById(integrantesCursos.Id_Usuario.Value);
                }
            }

            return integrantesCursos;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<IntegrantesCursos?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateIntegrantesCursos")]
        public async Task<ActionResult<IntegrantesCursos>> CreateIntegrantesCursos([FromBody] IntegrantesCursos integrantesCursos)
        {
            try
            {
                var result = await _service.Create(integrantesCursos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateIntegrantesCursos")]
        public async Task<ActionResult<IntegrantesCursos>> UpdateIntegrantesCursos([FromBody] IntegrantesCursos integrantesCursos)
        {
            try
            {
                var result = await _service.Update(integrantesCursos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteIntegrantesCursos/{id}")]
        public async Task<ActionResult> DeleteIntegrantesCursos(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var integrantesCursos = await _service.GetById(id);
                if (integrantesCursos != null)
                {
                    await _service.Delete(integrantesCursos);
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
        [Route("CreateAllIntegrantesCursos")]
        public async Task<ActionResult<List<IntegrantesCursos>>> CreateAllIntegrantesCursos([FromBody] List<IntegrantesCursos> integrantesCursos)
        {
            try
            {
                var result = await _service.CreateAll(integrantesCursos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllIntegrantesCursos")]
        public async Task<ActionResult<List<IntegrantesCursos>>> UpdateAllIntegrantesCursos([FromBody] List<IntegrantesCursos> integrantesCursos)
        {
            try
            {
                var result = await _service.UpdateAll(integrantesCursos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllIntegrantesCursos")]
        public async Task<ActionResult> DeleteAllIntegrantesCursos([FromBody] List<IntegrantesCursos> integrantesCursos)
        {
            try
            {
                var ids = integrantesCursos.Select(ic => ic.Id).ToList();

                foreach (var id in ids)
                {
                    var entity = await _service.GetById(id);
                    if (entity != null)
                    {
                        // Desconectar las entidades relacionadas antes de eliminar
                        entity.Curso = null;
                        entity.Usuario = null;

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

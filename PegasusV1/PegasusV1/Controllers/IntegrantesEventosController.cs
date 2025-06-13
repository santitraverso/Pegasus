using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IntegrantesEventosController : BaseSecureController<IntegrantesEventos>
    {
        private readonly IService<Evento> _eventoService;
        private readonly IService<Usuario> _usuarioService;

        public IntegrantesEventosController(
            IService<IntegrantesEventos> integrantesEventosService,
            IService<Evento> eventoService,
            IService<Usuario> usuarioService)
            : base(integrantesEventosService)
        {
            _eventoService = eventoService;
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetIntegrantesEventossForCombo")]
        public async Task<ActionResult<List<IntegrantesEventos>>> GetIntegrantesEventossForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<IntegrantesEventos>> GetForComboInternal(Expression<Func<IntegrantesEventos, bool>>? predicate)
        {
            return await _service.GetIntegrantesEventosForCombo(predicate);
        }

        protected override async Task<IntegrantesEventos?> GetByIdInternal(int id)
        {
            var integrantesEventos = await _service.GetById(id);

            if (integrantesEventos != null)
            {
                if (integrantesEventos.Id_Evento.HasValue)
                {
                    integrantesEventos.Evento = await _eventoService.GetById(integrantesEventos.Id_Evento.Value);
                }

                if (integrantesEventos.Id_Usuario.HasValue)
                {
                    integrantesEventos.Usuario = await _usuarioService.GetById(integrantesEventos.Id_Usuario.Value);
                }
            }

            return integrantesEventos;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<IntegrantesEventos?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateIntegrantesEventos")]
        public async Task<ActionResult<IntegrantesEventos>> CreateIntegrantesEventos([FromBody] IntegrantesEventos integrantesEventos)
        {
            try
            {
                var result = await _service.Create(integrantesEventos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateIntegrantesEventos")]
        public async Task<ActionResult<IntegrantesEventos>> UpdateIntegrantesEventos([FromBody] IntegrantesEventos integrantesEventos)
        {
            try
            {
                var result = await _service.Update(integrantesEventos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteIntegrantesEventos/{id}")]
        public async Task<ActionResult> DeleteIntegrantesEventos(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var integrantesEventos = await _service.GetById(id);
                if (integrantesEventos != null)
                {
                    await _service.Delete(integrantesEventos);
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
        [Route("CreateAllIntegrantesEventos")]
        public async Task<ActionResult<List<IntegrantesEventos>>> CreateAllIntegrantesEventos([FromBody] List<IntegrantesEventos> integrantesEventos)
        {
            try
            {
                var result = await _service.CreateAll(integrantesEventos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllIntegrantesEventos")]
        public async Task<ActionResult<List<IntegrantesEventos>>> UpdateAllIntegrantesEventos([FromBody] List<IntegrantesEventos> integrantesEventos)
        {
            try
            {
                var result = await _service.UpdateAll(integrantesEventos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllIntegrantesEventos")]
        public async Task<ActionResult> DeleteAllIntegrantesEventos([FromBody] List<IntegrantesEventos> integrantesEventos)
        {
            try
            {
                await _service.DeleteAll(integrantesEventos);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

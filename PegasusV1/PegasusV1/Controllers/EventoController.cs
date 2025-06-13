using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventoController : BaseSecureController<Evento>
    {
        public EventoController(IService<Evento> eventoService)
            : base(eventoService)
        {
        }

        [HttpGet]
        [Route("GetEventosForCombo")]
        public async Task<ActionResult<List<Evento>>> GetEventosForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<Evento?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateEvento")]
        public async Task<ActionResult<Evento>> CreateEvento([FromBody] Evento evento)
        {
            try
            {
                var result = await _service.Create(evento);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateEvento")]
        public async Task<ActionResult<Evento>> UpdateEvento([FromBody] Evento evento)
        {
            try
            {
                var result = await _service.Update(evento);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteEvento/{id}")]
        public async Task<ActionResult> DeleteEvento(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var evento = await _service.GetById(id);
                if (evento != null)
                {
                    await _service.Delete(evento);
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
        [Route("CreateAllEvento")]
        public async Task<ActionResult<List<Evento>>> CreateAllEvento([FromBody] List<Evento> eventos)
        {
            try
            {
                var result = await _service.CreateAll(eventos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllEvento")]
        public async Task<ActionResult<List<Evento>>> UpdateAllEvento([FromBody] List<Evento> eventos)
        {
            try
            {
                var result = await _service.UpdateAll(eventos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllEvento")]
        public async Task<ActionResult> DeleteAllEvento([FromBody] List<Evento> eventos)
        {
            try
            {
                await _service.DeleteAll(eventos);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

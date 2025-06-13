using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DesempenioController : BaseSecureController<Desempenio>
    {
        public DesempenioController(IService<Desempenio> desempenioService)
            : base(desempenioService)
        {
        }

        [HttpGet]
        [Route("GetDesempenosForCombo")]
        public async Task<ActionResult<List<Desempenio>>> GetDesempenosForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<Desempenio>> GetForComboInternal(Expression<Func<Desempenio, bool>>? predicate)
        {
            return await _service.GetDesempenoForCombo(predicate);
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<Desempenio?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateDesempeno")]
        public async Task<ActionResult<Desempenio>> CreateDesempeno([FromBody] Desempenio desempeno)
        {
            try
            {
                var result = await _service.Create(desempeno);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateDesempeno")]
        public async Task<ActionResult<Desempenio>> UpdateDesempeno([FromBody] Desempenio desempeno)
        {
            try
            {
                var result = await _service.Update(desempeno);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteDesempeno/{id}")]
        public async Task<ActionResult> DeleteDesempeno(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var desempeno = await _service.GetById(id);
                if (desempeno != null)
                {
                    await _service.Delete(desempeno);
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
        [Route("CreateAllDesempeno")]
        public async Task<ActionResult<List<Desempenio>>> CreateAllDesempeno([FromBody] List<Desempenio> desempenos)
        {
            try
            {
                var result = await _service.CreateAll(desempenos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllDesempeno")]
        public async Task<ActionResult<List<Desempenio>>> UpdateAllDesempeno([FromBody] List<Desempenio> desempenos)
        {
            try
            {
                var result = await _service.UpdateAll(desempenos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllDesempeno")]
        public async Task<ActionResult> DeleteAllDesempeno([FromBody] List<Desempenio> desempenos)
        {
            try
            {
                await _service.DeleteAll(desempenos);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

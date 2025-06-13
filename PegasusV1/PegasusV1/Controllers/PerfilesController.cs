using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PerfilesController : BaseSecureController<Perfiles>
    {
        public PerfilesController(IService<Perfiles> perfilesService)
            : base(perfilesService)
        {
        }

        [HttpGet]
        [Route("GetPerfilesForCombo")]
        public async Task<ActionResult<List<Perfiles>>> GetPerfilessForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<Perfiles>> GetForComboInternal(Expression<Func<Perfiles, bool>>? predicate)
        {
            return await _service.GetPerfilesForCombo(predicate);
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<Perfiles?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreatePerfiles")]
        public async Task<ActionResult<Perfiles>> CreatePerfiles([FromBody] Perfiles perfiles)
        {
            try
            {
                var result = await _service.Create(perfiles);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdatePerfiles")]
        public async Task<ActionResult<Perfiles>> UpdatePerfiles([FromBody] Perfiles perfiles)
        {
            try
            {
                var result = await _service.Update(perfiles);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeletePerfiles/{id}")]
        public async Task<ActionResult> DeletePerfiles(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var perfiles = await _service.GetById(id);
                if (perfiles != null)
                {
                    await _service.Delete(perfiles);
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
        [Route("CreateAllPerfiles")]
        public async Task<ActionResult<List<Perfiles>>> CreateAllPerfiles([FromBody] List<Perfiles> perfiles)
        {
            try
            {
                var result = await _service.CreateAll(perfiles);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllPerfiles")]
        public async Task<ActionResult<List<Perfiles>>> UpdateAllPerfiles([FromBody] List<Perfiles> perfiles)
        {
            try
            {
                var result = await _service.UpdateAll(perfiles);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllPerfiles")]
        public async Task<ActionResult> DeleteAllPerfiles([FromBody] List<Perfiles> perfiles)
        {
            try
            {
                await _service.DeleteAll(perfiles);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

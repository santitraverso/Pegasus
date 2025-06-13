using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ModulosController : BaseSecureController<Modulos>
    {
        public ModulosController(IService<Modulos> modulosService)
            : base(modulosService)
        {
        }

        [HttpGet]
        [Route("GetModulosForCombo")]
        public async Task<ActionResult<List<Modulos>>> GetModulossForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<Modulos>> GetForComboInternal(Expression<Func<Modulos, bool>>? predicate)
        {
            return await _service.GetModulosForCombo(predicate);
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<Modulos?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateModulos")]
        public async Task<ActionResult<Modulos>> CreateModulos([FromBody] Modulos modulos)
        {
            try
            {
                var result = await _service.Create(modulos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateModulos")]
        public async Task<ActionResult<Modulos>> UpdateModulos([FromBody] Modulos modulos)
        {
            try
            {
                var result = await _service.Update(modulos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteModulos/{id}")]
        public async Task<ActionResult> DeleteModulos(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var modulos = await _service.GetById(id);
                if (modulos != null)
                {
                    await _service.Delete(modulos);
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
        [Route("CreateAllModulos")]
        public async Task<ActionResult<List<Modulos>>> CreateAllModulos([FromBody] List<Modulos> modulos)
        {
            try
            {
                var result = await _service.CreateAll(modulos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllModulos")]
        public async Task<ActionResult<List<Modulos>>> UpdateAllModulos([FromBody] List<Modulos> modulos)
        {
            try
            {
                var result = await _service.UpdateAll(modulos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllModulos")]
        public async Task<ActionResult> DeleteAllModulos([FromBody] List<Modulos> modulos)
        {
            try
            {
                await _service.DeleteAll(modulos);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactosController : BaseSecureController<Contactos>
    {
        public ContactosController(IService<Contactos> contactoService)
            : base(contactoService)
        {
        }

        [HttpGet]
        [Route("GetContactosForCombo")]
        public async Task<ActionResult<List<Contactos>>> GetContactosForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<Contactos?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateContacto")]
        public async Task<ActionResult<Contactos>> CreateContacto([FromBody] Contactos contacto)
        {
            try
            {
                var result = await _service.Create(contacto);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateContacto")]
        public async Task<ActionResult<Contactos>> UpdateContacto([FromBody] Contactos contacto)
        {
            try
            {
                var result = await _service.Update(contacto);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteContacto/{id}")]
        public async Task<ActionResult> DeleteContacto(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var contacto = await _service.GetById(id);
                if (contacto != null)
                {
                    await _service.Delete(contacto);
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
        [Route("CreateAllContacto")]
        public async Task<ActionResult<List<Contactos>>> CreateAllContacto([FromBody] List<Contactos> contactos)
        {
            try
            {
                var result = await _service.CreateAll(contactos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllContacto")]
        public async Task<ActionResult<List<Contactos>>> UpdateAllContacto([FromBody] List<Contactos> contactos)
        {
            try
            {
                var result = await _service.UpdateAll(contactos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllContacto")]
        public async Task<ActionResult> DeleteAllContacto([FromBody] List<Contactos> contactos)
        {
            try
            {
                await _service.DeleteAll(contactos);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

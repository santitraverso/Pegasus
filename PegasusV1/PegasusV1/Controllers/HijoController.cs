using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HijoController : BaseSecureController<Hijo>
    {
        private readonly IService<Usuario> _usuarioService;

        public HijoController(
            IService<Hijo> hijoService,
            IService<Usuario> usuarioService)
            : base(hijoService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetHijosForCombo")]
        public async Task<ActionResult<List<Hijo>>> GetHijosForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<Hijo>> GetForComboInternal(Expression<Func<Hijo, bool>>? predicate)
        {
            return await _service.GetHijoForCombo(predicate);
        }

        protected override async Task<Hijo?> GetByIdInternal(int id)
        {
            var hijo = await _service.GetById(id);

            if (hijo != null)
            {
                if (hijo.Id_Hijo.HasValue)
                {
                    hijo.HijoUsuario = await _usuarioService.GetById(hijo.Id_Hijo.Value);
                }

                if (hijo.Id_Padre.HasValue)
                {
                    hijo.Padre = await _usuarioService.GetById(hijo.Id_Padre.Value);
                }
            }

            return hijo;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<Hijo?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateHijo")]
        public async Task<ActionResult<Hijo>> CreateHijo([FromBody] Hijo hijo)
        {
            try
            {
                var result = await _service.Create(hijo);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateHijo")]
        public async Task<ActionResult<Hijo>> UpdateHijo([FromBody] Hijo hijo)
        {
            try
            {
                var result = await _service.Update(hijo);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteHijo/{id}")]
        public async Task<ActionResult> DeleteHijo(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var hijo = await _service.GetById(id);
                if (hijo != null)
                {
                    await _service.Delete(hijo);
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
        [Route("CreateAllHijo")]
        public async Task<ActionResult<List<Hijo>>> CreateAllHijo([FromBody] List<Hijo> hijos)
        {
            try
            {
                var result = await _service.CreateAll(hijos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllHijo")]
        public async Task<ActionResult<List<Hijo>>> UpdateAllHijo([FromBody] List<Hijo> hijos)
        {
            try
            {
                var result = await _service.UpdateAll(hijos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllHijo")]
        public async Task<ActionResult> DeleteAllHijo([FromBody] List<Hijo> hijos)
        {
            try
            {
                await _service.DeleteAll(hijos);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

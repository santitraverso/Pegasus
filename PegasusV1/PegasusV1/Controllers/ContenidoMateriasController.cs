using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContenidoMateriasController : BaseSecureController<ContenidoMaterias>
    {
        private readonly IService<Materia> _materiaService;

        public ContenidoMateriasController(
            IService<ContenidoMaterias> contenidoMateriasService,
            IService<Materia> materiaService)
            : base(contenidoMateriasService)
        {
            _materiaService = materiaService;
        }

        [HttpGet]
        [Route("GetContenidoMateriasForCombo")]
        public async Task<ActionResult<List<ContenidoMaterias>>> GetContenidoMateriasForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<ContenidoMaterias>> GetForComboInternal(Expression<Func<ContenidoMaterias, bool>>? predicate)
        {
            return await _service.GetContenidoMateriasForCombo(predicate);
        }

        protected override async Task<ContenidoMaterias?> GetByIdInternal(int id)
        {
            var contenidoMaterias = await _service.GetById(id);

            if (contenidoMaterias != null)
            {
                if (contenidoMaterias.Id_Materia.HasValue)
                {
                    contenidoMaterias.Materia = await _materiaService.GetById(contenidoMaterias.Id_Materia.Value);
                }
            }

            return contenidoMaterias;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<ContenidoMaterias?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateContenidoMaterias")]
        public async Task<ActionResult<ContenidoMaterias>> CreateContenidoMaterias([FromBody] ContenidoMaterias contenidoMaterias)
        {
            try
            {
                var result = await _service.Create(contenidoMaterias);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateContenidoMaterias")]
        public async Task<ActionResult<ContenidoMaterias>> UpdateContenidoMaterias([FromBody] ContenidoMaterias contenidoMaterias)
        {
            try
            {
                var result = await _service.Update(contenidoMaterias);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteContenidoMaterias/{id}")]
        public async Task<ActionResult> DeleteContenidoMaterias(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var contenidoMaterias = await _service.GetById(id);
                if (contenidoMaterias != null)
                {
                    await _service.Delete(contenidoMaterias);
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
        [Route("CreateAllContenidoMaterias")]
        public async Task<ActionResult<List<ContenidoMaterias>>> CreateAllContenidoMaterias([FromBody] List<ContenidoMaterias> contenidoMaterias)
        {
            try
            {
                var result = await _service.CreateAll(contenidoMaterias);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllContenidoMaterias")]
        public async Task<ActionResult<List<ContenidoMaterias>>> UpdateAllContenidoMaterias([FromBody] List<ContenidoMaterias> contenidoMaterias)
        {
            try
            {
                var result = await _service.UpdateAll(contenidoMaterias);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllContenidoMaterias")]
        public async Task<ActionResult> DeleteAllContenidoMaterias([FromBody] List<ContenidoMaterias> contenidoMaterias)
        {
            try
            {
                await _service.DeleteAll(contenidoMaterias);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

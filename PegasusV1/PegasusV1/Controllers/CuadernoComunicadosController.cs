using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CuadernoComunicadosController : BaseSecureController<CuadernoComunicados>
    {
        private readonly IService<Usuario> _usuarioService;
        private readonly IService<Curso> _cursoService;

        public CuadernoComunicadosController(
            IService<CuadernoComunicados> cuadernoComunicadosService,
            IService<Usuario> usuarioService,
            IService<Curso> cursoService)
            : base(cuadernoComunicadosService)
        {
            _usuarioService = usuarioService;
            _cursoService = cursoService;
        }

        [HttpGet]
        [Route("GetCuadernoComunicadosForCombo")]
        public async Task<ActionResult<List<CuadernoComunicados>>> GetCuadernoComunicadosForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<CuadernoComunicados>> GetForComboInternal(Expression<Func<CuadernoComunicados, bool>>? predicate)
        {
            return await _service.GetCuadernoComunicadosForCombo(predicate);
        }

        protected override async Task<CuadernoComunicados?> GetByIdInternal(int id)
        {
            var cuaderno = await _service.GetById(id);

            if (cuaderno != null)
            {
                if (cuaderno.Id_Usuario.HasValue)
                {
                    cuaderno.Usuario = await _usuarioService.GetById(cuaderno.Id_Usuario.Value);
                }

                if (cuaderno.Id_Curso.HasValue)
                {
                    cuaderno.Curso = await _cursoService.GetById(cuaderno.Id_Curso.Value);
                }
            }

            return cuaderno;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<CuadernoComunicados?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateCuadernoComunicados")]
        public async Task<ActionResult<CuadernoComunicados>> CreateCuadernoComunicados([FromBody] CuadernoComunicados cuadernoComunicados)
        {
            try
            {
                var result = await _service.Create(cuadernoComunicados);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateCuadernoComunicados")]
        public async Task<ActionResult<CuadernoComunicados>> UpdateCuadernoComunicados([FromBody] CuadernoComunicados cuadernoComunicados)
        {
            try
            {
                var result = await _service.Update(cuadernoComunicados);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteCuadernoComunicados/{id}")]
        public async Task<ActionResult> DeleteCuadernoComunicados(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var cuadernoComunicados = await _service.GetById(id);
                if (cuadernoComunicados != null)
                {
                    await _service.Delete(cuadernoComunicados);
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
        [Route("CreateAllCuadernoComunicados")]
        public async Task<ActionResult<List<CuadernoComunicados>>> CreateAllCuadernoComunicados([FromBody] List<CuadernoComunicados> cuadernoComunicados)
        {
            try
            {
                var result = await _service.CreateAll(cuadernoComunicados);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllCuadernoComunicados")]
        public async Task<ActionResult<List<CuadernoComunicados>>> UpdateAllCuadernoComunicados([FromBody] List<CuadernoComunicados> cuadernoComunicados)
        {
            try
            {
                var result = await _service.UpdateAll(cuadernoComunicados);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllCuadernoComunicados")]
        public async Task<ActionResult> DeleteAllCuadernoComunicados([FromBody] List<CuadernoComunicados> cuadernoComunicados)
        {
            try
            {
                await _service.DeleteAll(cuadernoComunicados);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

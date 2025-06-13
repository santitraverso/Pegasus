using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ComunicadoAlumnosController : BaseSecureController<ComunicadoAlumnos>
    {
        private readonly IService<CuadernoComunicados> _cuadernoComunicadosService;
        private readonly IService<Usuario> _usuarioService;

        public ComunicadoAlumnosController(
            IService<ComunicadoAlumnos> comunicadoAlumnosService,
            IService<CuadernoComunicados> cuadernoComunicadosService,
            IService<Usuario> usuarioService)
            : base(comunicadoAlumnosService)
        {
            _cuadernoComunicadosService = cuadernoComunicadosService;
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetComunicadoAlumnossForCombo")]
        public async Task<ActionResult<List<ComunicadoAlumnos>>> GetComunicadoAlumnosForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<ComunicadoAlumnos>> GetForComboInternal(Expression<Func<ComunicadoAlumnos, bool>>? predicate)
        {
            var comunicadoAlumnos = await _service.GetComunicadoAlumnosForCombo(predicate);

            // Cargar las relaciones
            foreach (var item in comunicadoAlumnos)
            {
                if (item != null)
                {
                    if (item.Id_Alumno.HasValue)
                    {
                        item.Alumno = await _usuarioService.GetById(item.Id_Alumno.Value);
                    }

                    if (item.Id_Comunicado.HasValue)
                    {
                        item.Comunicado = await _cuadernoComunicadosService.GetById(item.Id_Comunicado.Value);
                    }
                }
            }

            return comunicadoAlumnos;
        }

        protected override async Task<ComunicadoAlumnos?> GetByIdInternal(int id)
        {
            var comunicadoAlumnos = await _service.GetById(id);

            if (comunicadoAlumnos != null)
            {
                if (comunicadoAlumnos.Id_Alumno.HasValue)
                {
                    comunicadoAlumnos.Alumno = await _usuarioService.GetById(comunicadoAlumnos.Id_Alumno.Value);
                }

                if (comunicadoAlumnos.Id_Comunicado.HasValue)
                {
                    comunicadoAlumnos.Comunicado = await _cuadernoComunicadosService.GetById(comunicadoAlumnos.Id_Comunicado.Value);
                }
            }

            return comunicadoAlumnos;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<ComunicadoAlumnos?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateComunicadoAlumnos")]
        public async Task<ActionResult<ComunicadoAlumnos>> CreateComunicadoAlumnos([FromBody] ComunicadoAlumnos comunicadoAlumnos)
        {
            try
            {
                var result = await _service.Create(comunicadoAlumnos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateComunicadoAlumnos")]
        public async Task<ActionResult<ComunicadoAlumnos>> UpdateComunicadoAlumnos([FromBody] ComunicadoAlumnos comunicadoAlumnos)
        {
            try
            {
                var result = await _service.Update(comunicadoAlumnos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteComunicadoAlumnos/{id}")]
        public async Task<ActionResult> DeleteComunicadoAlumnos(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var comunicadoAlumnos = await _service.GetById(id);
                if (comunicadoAlumnos != null)
                {
                    await _service.Delete(comunicadoAlumnos);
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
        [Route("CreateAllComunicadoAlumnos")]
        public async Task<ActionResult<List<ComunicadoAlumnos>>> CreateAllComunicadoAlumnos([FromBody] List<ComunicadoAlumnos> comunicadoAlumnos)
        {
            try
            {
                var result = await _service.CreateAll(comunicadoAlumnos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllComunicadoAlumnos")]
        public async Task<ActionResult<List<ComunicadoAlumnos>>> UpdateAllComunicadoAlumnos([FromBody] List<ComunicadoAlumnos> comunicadoAlumnos)
        {
            try
            {
                var result = await _service.UpdateAll(comunicadoAlumnos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllComunicadoAlumnos")]
        public async Task<ActionResult> DeleteAllComunicadoAlumnos([FromBody] List<ComunicadoAlumnos> comunicadoAlumnos)
        {
            try
            {
                await _service.DeleteAll(comunicadoAlumnos);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

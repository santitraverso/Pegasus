using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DesempenioAlumnosController : BaseSecureController<DesempenioAlumnos>
    {
        private readonly IService<Desempenio> _desempenioService;
        private readonly IService<Usuario> _usuarioService;

        public DesempenioAlumnosController(
            IService<DesempenioAlumnos> desempenioAlumnosService,
            IService<Usuario> usuarioService,
            IService<Desempenio> desempenioService)
            : base(desempenioAlumnosService)
        {
            _usuarioService = usuarioService;
            _desempenioService = desempenioService;
        }

        [HttpGet]
        [Route("GetDesempenioAlumnossForCombo")]
        public async Task<ActionResult<List<DesempenioAlumnos>>> GetDesempenioAlumnossForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<DesempenioAlumnos>> GetForComboInternal(Expression<Func<DesempenioAlumnos, bool>>? predicate)
        {
            var desempenioAlumnos = await _service.GetDesempenioAlumnosForCombo(predicate);

            foreach (var item in desempenioAlumnos)
            {
                if (item != null)
                {
                    if (item.Id_Alumno.HasValue)
                    {
                        item.Alumno = await _usuarioService.GetById(item.Id_Alumno.Value);
                    }

                    if (item.Promedio > 0)
                    {
                        var desempeno = await _desempenioService.GetDesempenoForCombo(d =>
                            item.Promedio >= d.PromedioMin &&
                            item.Promedio <= d.PromedioMax);

                        var desempenoResultado = desempeno.FirstOrDefault();
                        if (desempenoResultado != null)
                        {
                            item.Desempenio = desempenoResultado;
                        }
                    }
                }
            }

            return desempenioAlumnos;
        }

        protected override async Task<DesempenioAlumnos?> GetByIdInternal(int id)
        {
            var desempenioAlumnos = await _service.GetById(id);

            if (desempenioAlumnos != null)
            {
                if (desempenioAlumnos.Id_Alumno.HasValue)
                {
                    desempenioAlumnos.Alumno = await _usuarioService.GetById(desempenioAlumnos.Id_Alumno.Value);
                }

                if (desempenioAlumnos.Promedio > 0)
                {
                    var desempeno = await _desempenioService.GetDesempenoForCombo(d =>
                        desempenioAlumnos.Promedio >= d.PromedioMin &&
                        desempenioAlumnos.Promedio <= d.PromedioMax);

                    var desempenoResultado = desempeno.FirstOrDefault();
                    if (desempenoResultado != null)
                    {
                        desempenioAlumnos.Desempenio = desempenoResultado;
                    }
                }
            }

            return desempenioAlumnos;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<DesempenioAlumnos?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateDesempenioAlumnos")]
        public async Task<ActionResult<DesempenioAlumnos>> CreateDesempenioAlumnos([FromBody] DesempenioAlumnos desempenioAlumnos)
        {
            try
            {
                var result = await _service.Create(desempenioAlumnos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateDesempenioAlumnos")]
        public async Task<ActionResult<DesempenioAlumnos>> UpdateDesempenioAlumnos([FromBody] DesempenioAlumnos desempenioAlumnos)
        {
            try
            {
                var result = await _service.Update(desempenioAlumnos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteDesempenioAlumnos/{id}")]
        public async Task<ActionResult> DeleteDesempenioAlumnos(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var desempenioAlumnos = await _service.GetById(id);
                if (desempenioAlumnos != null)
                {
                    await _service.Delete(desempenioAlumnos);
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
        [Route("CreateAllDesempenioAlumnos")]
        public async Task<ActionResult<List<DesempenioAlumnos>>> CreateAllDesempenioAlumnos([FromBody] List<DesempenioAlumnos> desempenioAlumnos)
        {
            try
            {
                var result = await _service.CreateAll(desempenioAlumnos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllDesempenioAlumnos")]
        public async Task<ActionResult<List<DesempenioAlumnos>>> UpdateAllDesempenioAlumnos([FromBody] List<DesempenioAlumnos> desempenioAlumnos)
        {
            try
            {
                var result = await _service.UpdateAll(desempenioAlumnos);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllDesempenioAlumnos")]
        public async Task<ActionResult> DeleteAllDesempenioAlumnos([FromBody] List<DesempenioAlumnos> desempenioAlumnos)
        {
            try
            {
                await _service.DeleteAll(desempenioAlumnos);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalificacionesController : BaseSecureController<Calificaciones>
    {
        private readonly IService<Calificaciones> CalificacionesService;
        private readonly IService<Materia> MateriaService;
        private readonly IService<Usuario> UsuarioService;

        public CalificacionesController(
            IService<Calificaciones> calificacionesService,
            IService<Materia> materiaService,
            IService<Usuario> usuarioService)
            : base(calificacionesService)
        {
            CalificacionesService = calificacionesService;
            MateriaService = materiaService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetCalificacionesForCombo")]
        public async Task<ActionResult<List<Calificaciones>>> GetCalificacionesForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<Calificaciones>> GetForComboInternal(Expression<Func<Calificaciones, bool>>? predicate)
        {
            return await _service.GetCalificacionesForCombo(predicate);
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<Calificaciones?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        protected override async Task<Calificaciones?> GetByIdInternal(int id)
        {
            var calificaciones = await _service.GetById(id);

            if (calificaciones != null)
            {
                if (calificaciones.Id_Materia.HasValue)
                {
                    calificaciones.Materia = await MateriaService.GetById(calificaciones.Id_Materia.Value);
                }

                if (calificaciones.Id_Alumno.HasValue)
                {
                    calificaciones.Usuario = await UsuarioService.GetById(calificaciones.Id_Alumno.Value);
                }
            }

            return calificaciones;
        }

        

        [HttpPost]
        [Route("CreateCalificaciones")]
        public async Task<Calificaciones> CreateCalificaciones(Calificaciones Calificaciones)
        {
            return await CalificacionesService.Create(Calificaciones);
        }

        [HttpPut]
        [Route("UpdateCalificaciones")]
        public async Task<Calificaciones> UpdateCalificaciones(Calificaciones Calificaciones)
        {
            return await CalificacionesService.Update(Calificaciones);
        }

        [HttpDelete]
        [Route("DeleteCalificaciones/{id}")]
        public async Task<ActionResult> DeleteCalificaciones(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var calificacion = await _service.GetById(id);

                if (calificacion != null)
                {
                    await CalificacionesService.Delete(calificacion);
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
        [Route("CreateAllCalificaciones")]
        public async Task<List<Calificaciones>> CreateAllCalificaciones(List<Calificaciones> Calificaciones)
        {
            return await CalificacionesService.CreateAll(Calificaciones);
        }

        [HttpPut]
        [Route("UpdateAllCalificaciones")]
        public async Task<List<Calificaciones>> UpdateAllCalificaciones(List<Calificaciones> Calificaciones)
        {
            return await CalificacionesService.UpdateAll(Calificaciones);
        }

        [HttpDelete]
        [Route("DeleteAllCalificaciones")]
        public async Task<ActionResult> DeleteAllCalificaciones([FromBody] List<Calificaciones> calificaciones)
        {
            try
            {
                await CalificacionesService.DeleteAll(calificaciones);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

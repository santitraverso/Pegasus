using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalificacionesController : ControllerBase
    {
        private readonly ILogger<CalificacionesController> _logger;
        private readonly IService<Calificaciones> CalificacionesService;
        private readonly IService<Materia> MateriaService;
        private readonly IService<Usuario> UsuarioService;

        public CalificacionesController(ILogger<CalificacionesController> logger,
            IService<Calificaciones> calificacionesService,
            IService<Materia> materiaService,
            IService<Usuario> usuarioService)
        {
            _logger = logger;
            CalificacionesService = calificacionesService;
            MateriaService = materiaService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetCalificacionesForCombo")]
        public async Task<List<Calificaciones>> GetCalificacionesForCombo(string? query = null)
        {
            Expression<Func<Calificaciones, bool>> ex = null;
            Expression<Func<Calificaciones, bool>> i = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Calificaciones), query);
                var p2 = Expression.Parameter(typeof(Materia), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Calificaciones, bool>>)e;
            }

            List<Calificaciones> Calificacioness = await CalificacionesService.GetCalificacionesForCombo(ex);

            return Calificacioness;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Calificaciones?> GetById(int id)
        {
            Calificaciones Calificaciones = await CalificacionesService.GetById(id);

            if (Calificaciones != null)
            {
                if (Calificaciones.Id_Materia.HasValue)
                {
                    Calificaciones.Materia = await MateriaService.GetById(Calificaciones.Id_Materia.Value);
                }

                if (Calificaciones.Id_Alumno.HasValue)
                {
                    Calificaciones.Usuario = await UsuarioService.GetById(Calificaciones.Id_Alumno.Value);
                }
            }

            return Calificaciones;
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

        [HttpGet]
        [Route("DeleteCalificaciones")]
        public async Task DeleteCalificaciones(int id)
        {
            Calificaciones? Calificaciones = await CalificacionesService.GetById(id);
            if (Calificaciones != null)
                await CalificacionesService.Delete(Calificaciones);
        }

        [HttpPut]
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

        [HttpGet]
        [Route("DeleteAllCalificaciones")]
        public async Task DeleteAllCalificaciones(List<Calificaciones> Calificaciones)
        {
            await CalificacionesService.DeleteAll(Calificaciones);
        }
    }
}

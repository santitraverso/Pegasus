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
    public class TareaController : ControllerBase
    {
        private readonly ILogger<TareaController> _logger;
        private readonly IService<Tarea> TareaService;
        private readonly IService<Materia> MateriaService;
        private readonly IService<Usuario> UsuarioService;

        public TareaController(ILogger<TareaController> logger,
            IService<Tarea> tareaService,
            IService<Materia> materiaService,
            IService<Usuario> usuarioService)
        {
            _logger = logger;
            TareaService = tareaService;
            MateriaService = materiaService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetTareasForCombo")]
        public async Task<List<Tarea>> GetTareasForCombo(string? query = null)
        {
            Expression<Func<Tarea, bool>> ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Tarea), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Tarea, bool>>)e;
            }

            List<Tarea> Tareas = await TareaService.GetTareaForCombo(ex);

            return Tareas;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Tarea?> GetById(int id)
        {
            Tarea Tarea = await TareaService.GetById(id);

            if (Tarea != null)
            {
                if (Tarea.Id_Materia.HasValue)
                {
                    Tarea.Materia = await MateriaService.GetById(Tarea.Id_Materia.Value);
                }

                if (Tarea.Id_Alumno.HasValue)
                {
                    Tarea.Alumno = await UsuarioService.GetById(Tarea.Id_Alumno.Value);
                }
            }

            return Tarea;
        }

        [HttpPost]
        [Route("CreateTarea")]
        public async Task<Tarea> CreateTarea(Tarea tarea)
        {
            return await TareaService.Create(tarea);
        }

        [HttpPut]
        [Route("UpdateTarea")]
        public async Task<Tarea> UpdateTarea(Tarea tarea)
        {
            return await TareaService.Update(tarea);
        }

        [HttpGet]
        [Route("DeleteTarea")]
        public async Task DeleteTarea(int id)
        {
            Tarea? tarea = await TareaService.GetById(id);
            if(tarea != null)
                await TareaService.Delete(tarea);
        }
    }
}

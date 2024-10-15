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
    public class DesempenoAlumnosController : ControllerBase
    {
        private readonly ILogger<DesempenoAlumnosController> _logger;
        private readonly IService<DesempenoAlumnos> DesempenoAlumnosService;
        private readonly IService<Usuario> UsuarioService;

        public DesempenoAlumnosController(ILogger<DesempenoAlumnosController> logger,
            IService<DesempenoAlumnos> desempenoAlumnosService,
            IService<Usuario> usuarioService)
        {
            _logger = logger;
            DesempenoAlumnosService = desempenoAlumnosService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetDesempenoAlumnossForCombo")]
        public async Task<List<DesempenoAlumnos>> GetDesempenoAlumnossForCombo(string? query = null)
        {
            Expression<Func<DesempenoAlumnos, bool>> ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(DesempenoAlumnos), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<DesempenoAlumnos, bool>>)e;
            }

            List<DesempenoAlumnos> DesempenoAlumnoss = await DesempenoAlumnosService.GetDesempenoAlumnosForCombo(ex);

            return DesempenoAlumnoss;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<DesempenoAlumnos?> GetById(int id)
        {
            DesempenoAlumnos DesempenoAlumnos = await DesempenoAlumnosService.GetById(id);

            if (DesempenoAlumnos != null)
            {
                if (DesempenoAlumnos.Id_Alumno.HasValue)
                {
                    DesempenoAlumnos.Alumno = await UsuarioService.GetById(DesempenoAlumnos.Id_Alumno.Value);
                }
            }

            return DesempenoAlumnos;
        }

        [HttpPost]
        [Route("CreateDesempenoAlumnos")]
        public async Task<DesempenoAlumnos> CreateDesempenoAlumnos(DesempenoAlumnos DesempenoAlumnos)
        {
            return await DesempenoAlumnosService.Create(DesempenoAlumnos);
        }

        [HttpPut]
        [Route("UpdateDesempenoAlumnos")]
        public async Task<DesempenoAlumnos> UpdateDesempenoAlumnos(DesempenoAlumnos DesempenoAlumnos)
        {
            return await DesempenoAlumnosService.Update(DesempenoAlumnos);
        }

        [HttpGet]
        [Route("DeleteDesempenoAlumnos")]
        public async Task DeleteDesempenoAlumnos(int id)
        {
            DesempenoAlumnos? DesempenoAlumnos = await DesempenoAlumnosService.GetById(id);
            if (DesempenoAlumnos != null)
                await DesempenoAlumnosService.Delete(DesempenoAlumnos);
        }

        [HttpPut]
        [Route("CreateAllDesempenoAlumnos")]
        public async Task<List<DesempenoAlumnos>> CreateAllDesempenoAlumnos(List<DesempenoAlumnos> DesempenoAlumnoss)
        {
            return await DesempenoAlumnosService.CreateAll(DesempenoAlumnoss);
        }

        [HttpPut]
        [Route("UpdateAllDesempenoAlumnos")]
        public async Task<List<DesempenoAlumnos>> UpdateAllDesempenoAlumnos(List<DesempenoAlumnos> DesempenoAlumnoss)
        {
            return await DesempenoAlumnosService.UpdateAll(DesempenoAlumnoss);
        }

        [HttpGet]
        [Route("DeleteAllDesempenoAlumnos")]
        public async Task DeleteAllDesempenoAlumnos(List<DesempenoAlumnos> DesempenoAlumnoss)
        {
            await DesempenoAlumnosService.DeleteAll(DesempenoAlumnoss);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Web;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AsistenciaController : ControllerBase
    {
        private readonly ILogger<AsistenciaController> _logger;
        private readonly IService<Asistencia> AsistenciaService;
        private readonly IService<Materia> MateriaService;
        private readonly IService<Curso> CursoService;
        private readonly IService<Usuario> UsuarioService;

        public AsistenciaController(ILogger<AsistenciaController> logger, 
            IService<Asistencia> asistenciaService,
            IService<Materia> materiaService,
            IService<Curso> cursoService,
            IService<Usuario> usuarioService)
        {
            _logger = logger;
            AsistenciaService = asistenciaService;
            MateriaService = materiaService;
            CursoService = cursoService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetAsistenciasForCombo")]
        public async Task<List<Asistencia>> GetAsistenciasForCombo(string? query = null) 
        { 
            Expression<Func<Asistencia, bool>>? ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Asistencia), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Asistencia, bool>>)e;
            }

            List<Asistencia> asistencias = await AsistenciaService.GetAsistenciasForCombo(ex);
                       
            return asistencias;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Asistencia?> GetById(int id)
        {
            Asistencia asistencia = await AsistenciaService.GetById(id);

            if (asistencia != null)
            {
                if (asistencia.Id_Materia.HasValue)
                {
                    asistencia.Materia = await MateriaService.GetById(asistencia.Id_Materia.Value);
                }

                if (asistencia.Id_Alumno.HasValue)
                {
                    asistencia.Alumno = await UsuarioService.GetById(asistencia.Id_Alumno.Value);
                }


                if (asistencia.Id_Curso.HasValue)
                {
                    asistencia.Curso = await CursoService.GetById(asistencia.Id_Curso.Value);
                }
            }

            return asistencia;
        }

        [HttpPost]
        [Route("CreateAsistencia")]
        public async Task<Asistencia> CreateAsistencia(Asistencia asistencia)
        {
            return await AsistenciaService.Create(asistencia);
        }

        [HttpPut]
        [Route("UpdateAsistencia")]
        public async Task<Asistencia> UpdateAsistencia(Asistencia asistencia)
        {
            return await AsistenciaService.Update(asistencia);
        }

        [HttpGet]
        [Route("DeleteAsistencia")]
        public async Task DeleteAsistencia(int id)
        {
            Asistencia? asistencia = await AsistenciaService.GetById(id);
            if(asistencia != null)
                await AsistenciaService.Delete(asistencia);
        }

        [HttpPut]
        [Route("CreateAllAsistencia")]
        public async Task<List<Asistencia>> CreateAllAsistencia(List<Asistencia> asistencias)
        {
            return await AsistenciaService.CreateAll(asistencias);
        }

        [HttpPut]
        [Route("UpdateAllAsistencia")]
        public async Task<List<Asistencia>> UpdateAllAsistencia(List<Asistencia> asistencias)
        {
            return await AsistenciaService.UpdateAll(asistencias);
        }

        [HttpGet]
        [Route("DeleteAllAsistencia")]
        public async Task DeleteAllAsistencia(List<Asistencia> asistencias)
        {
            await AsistenciaService.DeleteAll(asistencias);
        }

    }
}

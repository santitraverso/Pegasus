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
    public class AsistenciaController : ControllerBase
    {
        private readonly ILogger<AsistenciaController> _logger;
        private readonly IService<Asistencia> AsistenciaService;
        private readonly IService<Materia> MateriaService;
        private readonly IService<Usuario> UsuarioService;

        public AsistenciaController(ILogger<AsistenciaController> logger, 
            IService<Asistencia> asistenciaService,
            IService<Materia> materiaService,
            IService<Usuario> usuarioService)
        {
            _logger = logger;
            AsistenciaService = asistenciaService;
            MateriaService = materiaService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetAsistenciasForCombo")]
        public async Task<List<Asistencia>> GetAsistenciasForCombo(string? query = null)
        {
            Expression<Func<Asistencia, bool>> ex = null;
            Expression<Func<Asistencia, bool>> i = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Asistencia), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Asistencia, bool>>)e;
            }

            List<Asistencia> asistencias = await AsistenciaService.GetForCombo(ex);

            foreach(Asistencia asistencia in asistencias)
            {
                 if (asistencia.Id_Materia.HasValue)
                 {
                     asistencia.Materia = await MateriaService.GetById(asistencia.Id_Materia.Value);
                 }

                 if (asistencia.Id_Alumno.HasValue)
                 {
                     asistencia.Alumno = await UsuarioService.GetById(asistencia.Id_Alumno.Value);
                 }
            }
           
           /*new Expression<Func<Asistencia, object>>[]
                {
                x => (x as Asistencia).Alumno, x => (x as Asistencia).Materia
                }*/
             
            return asistencias;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Asistencia> GetById(int id)
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
            }

            return asistencia;
        }

        [HttpPost]
        [Route("CreateAsistencia")]
        public async Task<Asistencia> CreateAsistencia(string asistencias)
        {
            Asistencia Asistencia = JsonConvert.DeserializeObject<Asistencia>(asistencias);
            return await AsistenciaService.Create(Asistencia);
        }

        [HttpPost]
        [Route("UpdateAsistencia")]
        public async Task<Asistencia> UpdateAsistencia(string asistencias)
        {
            Asistencia Asistencia = JsonConvert.DeserializeObject<Asistencia>(asistencias);
            return await AsistenciaService.Update(Asistencia);
        }

        [HttpPost]
        [Route("DeleteAsistencia")]
        public async void DeleteAsistencia(string asistencias)
        {
            Asistencia Asistencia = JsonConvert.DeserializeObject<Asistencia>(asistencias);
            AsistenciaService.Delete(Asistencia);
        }
    }
}

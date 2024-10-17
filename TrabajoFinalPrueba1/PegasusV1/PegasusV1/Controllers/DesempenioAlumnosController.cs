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
    public class DesempenioAlumnosController : ControllerBase
    {
        private readonly ILogger<DesempenioAlumnosController> _logger;
        private readonly IService<DesempenioAlumnos> DesempenioAlumnosService;
        private readonly IService<Desempenio> DesempenioService;
        private readonly IService<Usuario> UsuarioService;

        public DesempenioAlumnosController(ILogger<DesempenioAlumnosController> logger,
            IService<DesempenioAlumnos> desempenioAlumnosService,
            IService<Usuario> usuarioService,
            IService<Desempenio> desempenoService)
        {
            _logger = logger;
            DesempenioAlumnosService = desempenioAlumnosService;
            UsuarioService = usuarioService;
            DesempenioService = desempenoService;
        }

        [HttpGet]
        [Route("GetDesempenioAlumnossForCombo")]
        public async Task<List<DesempenioAlumnos>> GetDesempenioAlumnossForCombo(string? query = null)
        {
            Expression<Func<DesempenioAlumnos, bool>> ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(DesempenioAlumnos), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<DesempenioAlumnos, bool>>)e;
            }

            // Obtener lista de DesempenioAlumnos basado en el query
            List<DesempenioAlumnos> DesempenioAlumnoss = await DesempenioAlumnosService.GetDesempenioAlumnosForCombo(ex);

            foreach (DesempenioAlumnos DesempenoAlumno in DesempenioAlumnoss)
            {
                if (DesempenoAlumno != null)
                {
                    // Obtener detalles del Alumno si tiene Id_Alumno
                    if (DesempenoAlumno.Id_Alumno.HasValue)
                    {
                        DesempenoAlumno.Alumno = await UsuarioService.GetById(DesempenoAlumno.Id_Alumno.Value);
                    }

                    // Si tiene un promedio mayor a 0, obtener la descripción del Desempeno
                    if (DesempenoAlumno.Promedio > 0)
                    {
                        var desempeno = await DesempenioService.GetDesempenoForCombo(d =>
                            DesempenoAlumno.Promedio >= d.PromedioMin &&
                            DesempenoAlumno.Promedio <= d.PromedioMax);

                        // Asignar la descripción obtenida de la tabla Desempeno
                        var desempenoResultado = desempeno.FirstOrDefault();
                        if (desempenoResultado != null)
                        {
                            DesempenoAlumno.Desempenio = desempenoResultado;
                        }
                    }
                }
            }

            return DesempenioAlumnoss;
        }


        [HttpGet]
        [Route("GetById")]
        public async Task<DesempenioAlumnos?> GetById(int id)
        {
            DesempenioAlumnos DesempenioAlumnos = await DesempenioAlumnosService.GetById(id);

            if (DesempenioAlumnos != null)
            {
                if (DesempenioAlumnos.Id_Alumno.HasValue)
                {
                    DesempenioAlumnos.Alumno = await UsuarioService.GetById(DesempenioAlumnos.Id_Alumno.Value);
                }
            }

            return DesempenioAlumnos;
        }

        [HttpPost]
        [Route("CreateDesempenioAlumnos")]
        public async Task<DesempenioAlumnos> CreateDesempenioAlumnos(DesempenioAlumnos DesempenioAlumnos)
        {
            return await DesempenioAlumnosService.Create(DesempenioAlumnos);
        }

        [HttpPut]
        [Route("UpdateDesempenioAlumnos")]
        public async Task<DesempenioAlumnos> UpdateDesempenioAlumnos(DesempenioAlumnos DesempenioAlumnos)
        {
            return await DesempenioAlumnosService.Update(DesempenioAlumnos);
        }

        [HttpGet]
        [Route("DeleteDesempenioAlumnos")]
        public async Task DeleteDesempenioAlumnos(int id)
        {
            DesempenioAlumnos? DesempenioAlumnos = await DesempenioAlumnosService.GetById(id);
            if (DesempenioAlumnos != null)
                await DesempenioAlumnosService.Delete(DesempenioAlumnos);
        }

        [HttpPut]
        [Route("CreateAllDesempenioAlumnos")]
        public async Task<List<DesempenioAlumnos>> CreateAllDesempenioAlumnos(List<DesempenioAlumnos> DesempenioAlumnoss)
        {
            return await DesempenioAlumnosService.CreateAll(DesempenioAlumnoss);
        }

        [HttpPut]
        [Route("UpdateAllDesempenioAlumnos")]
        public async Task<List<DesempenioAlumnos>> UpdateAllDesempenioAlumnos(List<DesempenioAlumnos> DesempenioAlumnoss)
        {
            return await DesempenioAlumnosService.UpdateAll(DesempenioAlumnoss);
        }

        [HttpGet]
        [Route("DeleteAllDesempenioAlumnos")]
        public async Task DeleteAllDesempenioAlumnos(List<DesempenioAlumnos> DesempenioAlumnoss)
        {
            await DesempenioAlumnosService.DeleteAll(DesempenioAlumnoss);
        }
    }
}

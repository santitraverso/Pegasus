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
    public class ComunicadoAlumnosController : ControllerBase
    {
        private readonly ILogger<ComunicadoAlumnosController> _logger;
        private readonly IService<ComunicadoAlumnos> ComunicadoAlumnosService;
        private readonly IService<CuadernoComunicados> CuadernoComunicadosService;
        private readonly IService<Usuario> UsuarioService;

        public ComunicadoAlumnosController(ILogger<ComunicadoAlumnosController> logger,
            IService<ComunicadoAlumnos> comunicadoAlumnosService,
            IService<CuadernoComunicados> cuadernoComunicadosService,
            IService<Usuario> usuarioService)
        {
            _logger = logger;
            ComunicadoAlumnosService = comunicadoAlumnosService;
            CuadernoComunicadosService = cuadernoComunicadosService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetComunicadoAlumnossForCombo")]
        public async Task<List<ComunicadoAlumnos>> GetComunicadoAlumnossForCombo(string? query = null)
        {
            Expression<Func<ComunicadoAlumnos, bool>> ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(ComunicadoAlumnos), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<ComunicadoAlumnos, bool>>)e;
            }

            List<ComunicadoAlumnos> ComunicadoAlumnoss = await ComunicadoAlumnosService.GetComunicadoAlumnosForCombo(ex);

            foreach (ComunicadoAlumnos ComunicadoAlumnos in ComunicadoAlumnoss)
            {
                if (ComunicadoAlumnos != null)
                {
                    if (ComunicadoAlumnos.Id_Alumno.HasValue)
                    {
                        ComunicadoAlumnos.Alumno = await UsuarioService.GetById(ComunicadoAlumnos.Id_Alumno.Value);
                    }

                    if (ComunicadoAlumnos.Id_Comunicado.HasValue)
                    {
                        ComunicadoAlumnos.Comunicado = await CuadernoComunicadosService.GetById(ComunicadoAlumnos.Id_Comunicado.Value);
                    }
                }
            }

            return ComunicadoAlumnoss;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ComunicadoAlumnos?> GetById(int id)
        {
            ComunicadoAlumnos ComunicadoAlumnos = await ComunicadoAlumnosService.GetById(id);

            if (ComunicadoAlumnos != null)
            {
                if (ComunicadoAlumnos.Id_Alumno.HasValue)
                {
                    ComunicadoAlumnos.Alumno = await UsuarioService.GetById(ComunicadoAlumnos.Id_Alumno.Value);
                }

                if (ComunicadoAlumnos.Id_Comunicado.HasValue)
                {
                    ComunicadoAlumnos.Comunicado = await CuadernoComunicadosService.GetById(ComunicadoAlumnos.Id_Comunicado.Value);
                }
            }

            return ComunicadoAlumnos;
        }

        [HttpPost]
        [Route("CreateComunicadoAlumnos")]
        public async Task<ComunicadoAlumnos> CreateComunicadoAlumnos(ComunicadoAlumnos ComunicadoAlumnos)
        {
            return await ComunicadoAlumnosService.Create(ComunicadoAlumnos);
        }

        [HttpPut]
        [Route("UpdateComunicadoAlumnos")]
        public async Task<ComunicadoAlumnos> UpdateComunicadoAlumnos(ComunicadoAlumnos ComunicadoAlumnos)
        {
            return await ComunicadoAlumnosService.Update(ComunicadoAlumnos);
        }

        [HttpGet]
        [Route("DeleteComunicadoAlumnos")]
        public async Task DeleteComunicadoAlumnos(int id)
        {
            ComunicadoAlumnos? ComunicadoAlumnos = await ComunicadoAlumnosService.GetById(id);
            if (ComunicadoAlumnos != null)
                await ComunicadoAlumnosService.Delete(ComunicadoAlumnos);
        }

        [HttpPut]
        [Route("CreateAllComunicadoAlumnos")]
        public async Task<List<ComunicadoAlumnos>> CreateAllComunicadoAlumnos(List<ComunicadoAlumnos> ComunicadoAlumnoss)
        {
            return await ComunicadoAlumnosService.CreateAll(ComunicadoAlumnoss);
        }

        [HttpPut]
        [Route("UpdateAllComunicadoAlumnos")]
        public async Task<List<ComunicadoAlumnos>> UpdateAllComunicadoAlumnos(List<ComunicadoAlumnos> ComunicadoAlumnoss)
        {
            return await ComunicadoAlumnosService.UpdateAll(ComunicadoAlumnoss);
        }

        [HttpGet]
        [Route("DeleteAllComunicadoAlumnos")]
        public async Task DeleteAllComunicadoAlumnos(List<ComunicadoAlumnos> ComunicadoAlumnoss)
        {
            await ComunicadoAlumnosService.DeleteAll(ComunicadoAlumnoss);
        }
    }
}

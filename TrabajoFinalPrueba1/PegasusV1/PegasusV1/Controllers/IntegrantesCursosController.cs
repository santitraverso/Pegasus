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
    public class IntegrantesCursosController : ControllerBase
    {
        private readonly ILogger<IntegrantesCursosController> _logger;
        private readonly IService<IntegrantesCursos> IntegrantesCursosService;
        private readonly IService<Curso> CursoService;
        private readonly IService<Usuario> UsuarioService;

        public IntegrantesCursosController(ILogger<IntegrantesCursosController> logger,
            IService<IntegrantesCursos> integrantesCursosService,
            IService<Curso> cursoService,
            IService<Usuario> usuarioService)
        {
            _logger = logger;
            IntegrantesCursosService = integrantesCursosService;
            CursoService = cursoService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetIntegrantesCursosForCombo")]
        public async Task<List<IntegrantesCursos>> GetIntegrantesCursossForCombo(string? query = null)
        {
            Expression<Func<IntegrantesCursos, bool>> ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(IntegrantesCursos), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<IntegrantesCursos, bool>>)e;
            }

            List<IntegrantesCursos> IntegrantesCursoss = await IntegrantesCursosService.GetIntegrantesCursosForCombo(ex);

            foreach (IntegrantesCursos integrante in IntegrantesCursoss)
            {
                if (integrante.Id_Curso.HasValue)
                {
                    integrante.Curso = await CursoService.GetById(integrante.Id_Curso.Value);
                }

                if (integrante.Id_Usuario.HasValue)
                {
                    integrante.Usuario = await UsuarioService.GetById(integrante.Id_Usuario.Value);
                }
            }

            return IntegrantesCursoss;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<IntegrantesCursos?> GetById(int id)
        {
            IntegrantesCursos IntegrantesCursos = await IntegrantesCursosService.GetById(id);

            if (IntegrantesCursos != null)
            {
                if (IntegrantesCursos.Id_Curso.HasValue)
                {
                    IntegrantesCursos.Curso = await CursoService.GetById(IntegrantesCursos.Id_Curso.Value);
                }

                if (IntegrantesCursos.Id_Usuario.HasValue)
                {
                    IntegrantesCursos.Usuario = await UsuarioService.GetById(IntegrantesCursos.Id_Usuario.Value);
                }
            }

            return IntegrantesCursos;
        }

        [HttpPost]
        [Route("CreateIntegrantesCursos")]
        public async Task<IntegrantesCursos> CreateIntegrantesCursos(IntegrantesCursos integrantesCursos)
        {
            return await IntegrantesCursosService.Create(integrantesCursos);
        }

        [HttpPut]
        [Route("UpdateIntegrantesCursos")]
        public async Task<IntegrantesCursos> UpdateIntegrantesCursos(IntegrantesCursos integrantesCursos)
        {
            return await IntegrantesCursosService.Update(integrantesCursos);
        }

        [HttpGet]
        [Route("DeleteIntegrantesCursos")]
        public async Task DeleteIntegrantesCursos(int id)
        {
            IntegrantesCursos? integrantesCursos = await IntegrantesCursosService.GetById(id);
            if (integrantesCursos != null)
                await IntegrantesCursosService.Delete(integrantesCursos);
        }

        [HttpPut]
        [Route("CreateAllIntegrantesCursos")]
        public async Task<List<IntegrantesCursos>> CreateAllIntegrantesCursos(List<IntegrantesCursos> integrantesCursos)
        {
            return await IntegrantesCursosService.CreateAll(integrantesCursos);
        }

        [HttpPut]
        [Route("UpdateAllIntegrantesCursos")]
        public async Task<List<IntegrantesCursos>> UpdateAllIntegrantesCursos(List<IntegrantesCursos> integrantesCursos)
        {
            return await IntegrantesCursosService.UpdateAll(integrantesCursos);
        }

        [HttpGet]
        [Route("DeleteAllIntegrantesCursos")]
        public async Task DeleteAllIntegrantesCursos(List<IntegrantesCursos> integrantesCursos)
        {
            await IntegrantesCursosService.DeleteAll(integrantesCursos);
        }
    }
}

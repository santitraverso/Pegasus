using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Web;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CursoController : ControllerBase
    {
        private readonly ILogger<CursoController> _logger;
        private readonly IService<Curso> CursoService;

        public CursoController(ILogger<CursoController> logger,
            IService<Curso> cursoService)
        {
            _logger = logger;
            CursoService = cursoService;
        }

        [HttpGet]
        [Route("GetCursosForCombo")]
        public async Task<List<Curso>> GetCursosForCombo(string? query = null)
        {
            Expression<Func<Curso, bool>>? ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Curso), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Curso, bool>>)e;
            }

            List<Curso> Cursos = await CursoService.GetCursoForCombo(ex);

            return Cursos;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Curso?> GetById(int id)
        {
            Curso Curso = await CursoService.GetById(id);

            return Curso;
        }

        [HttpPost]
        [Route("CreateCurso")]
        public async Task<Curso> CreateCurso(Curso Curso)
        {
            return await CursoService.Create(Curso);
        }

        [HttpPut]
        [Route("UpdateCurso")]
        public async Task<Curso> UpdateCurso(Curso Curso)
        {
            return await CursoService.Update(Curso);
        }

        [HttpGet]
        [Route("DeleteCurso")]
        public async Task DeleteCurso(int id)
        {
            Curso? Curso = await CursoService.GetById(id);
            if (Curso != null)
                await CursoService.Delete(Curso);
        }

        [HttpPut]
        [Route("CreateAllCurso")]
        public async Task<List<Curso>> CreateAllCurso(List<Curso> Cursos)
        {
            return await CursoService.CreateAll(Cursos);
        }

        [HttpPut]
        [Route("UpdateAllCurso")]
        public async Task<List<Curso>> UpdateAllCurso(List<Curso> Cursos)
        {
            return await CursoService.UpdateAll(Cursos);
        }

        [HttpGet]
        [Route("DeleteAllCurso")]
        public async Task DeleteAllCurso(List<Curso> Cursos)
        {
            await CursoService.DeleteAll(Cursos);
        }

    }
}

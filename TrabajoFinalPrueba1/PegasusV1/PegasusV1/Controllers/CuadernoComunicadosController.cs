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
    public class CuadernoComunicadosController : ControllerBase
    {
        private readonly ILogger<CuadernoComunicadosController> _logger;
        private readonly IService<CuadernoComunicados> CuadernoComunicadosService;
        private readonly IService<Usuario> UsuarioService;
        private readonly IService<Curso> CursoService;

        public CuadernoComunicadosController(ILogger<CuadernoComunicadosController> logger,
            IService<CuadernoComunicados> cuadernoComunicadosService,
            IService<Usuario> usuarioService,
            IService<Curso> cursoService)
        {
            _logger = logger;
            CuadernoComunicadosService = cuadernoComunicadosService;
            UsuarioService = usuarioService;
            CursoService = cursoService;
        }

        [HttpGet]
        [Route("GetCuadernoComunicadosForCombo")]
        public async Task<List<CuadernoComunicados>> GetCuadernoComunicadosForCombo(string? query = null)
        {
            Expression<Func<CuadernoComunicados, bool>> ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(CuadernoComunicados), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<CuadernoComunicados, bool>>)e;
            }

            List<CuadernoComunicados> CuadernoComunicadoss = await CuadernoComunicadosService.GetCuadernoComunicadosForCombo(ex);

            return CuadernoComunicadoss;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<CuadernoComunicados?> GetById(int id)
        {
            CuadernoComunicados cuaderno = await CuadernoComunicadosService.GetById(id);

            if(cuaderno != null)
            {
                if (cuaderno.Id_Usuario.HasValue)
                {
                    cuaderno.Usuario = await UsuarioService.GetById(cuaderno.Id_Usuario.Value);
                }

                if (cuaderno.Id_Curso.HasValue)
                {
                    cuaderno.Curso = await CursoService.GetById(cuaderno.Id_Curso.Value);
                }
            }

            return cuaderno;
        }

        [HttpPost]
        [Route("CreateCuadernoComunicados")]
        public async Task<CuadernoComunicados> CreateCuadernoComunicados(CuadernoComunicados cuadernoComunicados)
        {
            return await CuadernoComunicadosService.Create(cuadernoComunicados);
        }

        [HttpPut]
        [Route("UpdateCuadernoComunicados")]
        public async Task<CuadernoComunicados> UpdateCuadernoComunicados(CuadernoComunicados cuadernoComunicados)
        {
            return await CuadernoComunicadosService.Update(cuadernoComunicados);
        }

        [HttpGet]
        [Route("DeleteCuadernoComunicados")]
        public async Task DeleteCuadernoComunicados(int id)
        {
            CuadernoComunicados? cuadernoComunicados = await CuadernoComunicadosService.GetById(id);
            if (cuadernoComunicados != null)
                await CuadernoComunicadosService.Delete(cuadernoComunicados);
        }

        [HttpPut]
        [Route("CreateAllCuadernoComunicados")]
        public async Task<List<CuadernoComunicados>> CreateAllCuadernoComunicados(List<CuadernoComunicados> cuadernoComunicados)
        {
            return await CuadernoComunicadosService.CreateAll(cuadernoComunicados);
        }

        [HttpPut]
        [Route("UpdateAllCuadernoComunicados")]
        public async Task<List<CuadernoComunicados>> UpdateAllCuadernoComunicados(List<CuadernoComunicados> cuadernoComunicados)
        {
            return await CuadernoComunicadosService.UpdateAll(cuadernoComunicados);
        }

        [HttpGet]
        [Route("DeleteAllCuadernoComunicados")]
        public async Task DeleteAllCuadernoComunicados(List<CuadernoComunicados> cuadernoComunicados)
        {
            await CuadernoComunicadosService.DeleteAll(cuadernoComunicados);
        }
    }
}

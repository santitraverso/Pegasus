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

        public CuadernoComunicadosController(ILogger<CuadernoComunicadosController> logger,
            IService<CuadernoComunicados> cuadernoComunicadosService,
            IService<Usuario> usuarioService)
        {
            _logger = logger;
            CuadernoComunicadosService = cuadernoComunicadosService;
            UsuarioService = usuarioService;
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

            List<CuadernoComunicados> CuadernoComunicadoss = await CuadernoComunicadosService.GetForCombo(ex);

            foreach (CuadernoComunicados CuadernoComunicados in CuadernoComunicadoss)
            {
                if (CuadernoComunicados.Id_Profesor.HasValue)
                {
                    CuadernoComunicados.Profesor = await UsuarioService.GetById(CuadernoComunicados.Id_Profesor.Value);
                }

                if (CuadernoComunicados.Id_Alumno.HasValue)
                {
                    CuadernoComunicados.Alumno = await UsuarioService.GetById(CuadernoComunicados.Id_Alumno.Value);
                }
            }

            return CuadernoComunicadoss;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<CuadernoComunicados?> GetById(int id)
        {
            CuadernoComunicados cuaderno = await CuadernoComunicadosService.GetById(id);

            if(cuaderno != null)
            {
                if (cuaderno.Id_Profesor.HasValue)
                {
                    cuaderno.Profesor = await UsuarioService.GetById(cuaderno.Id_Profesor.Value);
                }

                if (cuaderno.Id_Alumno.HasValue)
                {
                    cuaderno.Alumno = await UsuarioService.GetById(cuaderno.Id_Alumno.Value);
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
            CuadernoComunicados? cuadernoComunicados = await GetById(id);
            if (cuadernoComunicados != null)
                await CuadernoComunicadosService.Delete(cuadernoComunicados);
        }
    }
}

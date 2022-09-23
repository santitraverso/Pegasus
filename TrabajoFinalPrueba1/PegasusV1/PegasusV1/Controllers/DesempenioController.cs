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
    public class DesempenioController : ControllerBase
    {
        private readonly ILogger<DesempenioController> _logger;
        private readonly IService<Desempenio> DesempenioService;
        private readonly IService<Usuario> UsuarioService;

        public DesempenioController(ILogger<DesempenioController> logger,
            IService<Desempenio> desempenioService,
            IService<Usuario> usuarioService)
        {
            _logger = logger;
            DesempenioService = desempenioService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetDesempeniosForCombo")]
        public async Task<List<Desempenio>> GetDesempeniosForCombo(string? query = null)
        {
            Expression<Func<Desempenio, bool>> ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Desempenio), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Desempenio, bool>>)e;
            }

            List<Desempenio> desempenios = await DesempenioService.GetForCombo(ex);

            foreach (Desempenio Desempenio in desempenios)
            {
                if (Desempenio.Id_Alumno.HasValue)
                {
                    Desempenio.Alumno = await UsuarioService.GetById(Desempenio.Id_Alumno.Value);
                }
            }

            return desempenios;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Desempenio?> GetById(int id)
        {
            Desempenio desempenio = await DesempenioService.GetById(id);

            if (desempenio != null)
            {
                if (desempenio.Id_Alumno.HasValue)
                {
                    desempenio.Alumno = await UsuarioService.GetById(desempenio.Id_Alumno.Value);
                }
            }

            return desempenio;
        }

        [HttpPost]
        [Route("CreateDesempenio")]
        public async Task<Desempenio> CreateDesempenio(Desempenio desempenio)
        {
            return await DesempenioService.Create(desempenio);
        }

        [HttpPut]
        [Route("UpdateDesempenio")]
        public async Task<Desempenio> UpdateDesempenio(Desempenio desempenio)
        {
            return await DesempenioService.Update(desempenio);
        }

        [HttpGet]
        [Route("DeleteDesempenio")]
        public async Task DeleteDesempenio(int id)
        {
            Desempenio? desempenio = await GetById(id);
            if(desempenio != null)
                await DesempenioService.Delete(desempenio);
        }
    }
}

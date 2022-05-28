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
            Expression<Func<Desempenio, bool>> i = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Desempenio), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Desempenio, bool>>)e;
            }

            List<Desempenio> Desempenios = await DesempenioService.GetForCombo(ex);

            foreach (Desempenio Desempenio in Desempenios)
            {
                if (Desempenio.Id_Alumno.HasValue)
                {
                    Desempenio.Alumno = await UsuarioService.GetById(Desempenio.Id_Alumno.Value);
                }
            }

            return Desempenios;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Desempenio> GetById(int id)
        {
            Desempenio Desempenio = await DesempenioService.GetById(id);

            if (Desempenio != null)
            {
                if (Desempenio.Id_Alumno.HasValue)
                {
                    Desempenio.Alumno = await UsuarioService.GetById(Desempenio.Id_Alumno.Value);
                }
            }

            return Desempenio;
        }

        [HttpPost]
        [Route("CreateDesempenio")]
        public async Task<Desempenio> CreateDesempenio(string desempenio)
        {
            Desempenio Desempenio = JsonConvert.DeserializeObject<Desempenio>(desempenio);
            return await DesempenioService.Create(Desempenio);
        }

        [HttpPost]
        [Route("UpdateDesempenio")]
        public async Task<Desempenio> UpdateDesempenio(string desempenio)
        {
            Desempenio Desempenio = JsonConvert.DeserializeObject<Desempenio>(desempenio);
            return await DesempenioService.Update(Desempenio);
        }

        [HttpPost]
        [Route("DeleteDesempenio")]
        public async void DeleteDesempenio(string desempenio)
        {
            Desempenio Desempenio = JsonConvert.DeserializeObject<Desempenio>(desempenio);
            DesempenioService.Delete(Desempenio);
        }
    }
}

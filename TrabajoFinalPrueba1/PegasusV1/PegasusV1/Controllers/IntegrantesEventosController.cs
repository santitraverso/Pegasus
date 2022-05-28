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
    public class IntegrantesEventosController : ControllerBase
    {
        private readonly ILogger<IntegrantesEventosController> _logger;
        private readonly IService<IntegrantesEventos> IntegrantesEventosService;
        private readonly IService<Evento> EventoService;
        private readonly IService<Usuario> UsuarioService;

        public IntegrantesEventosController(ILogger<IntegrantesEventosController> logger,
            IService<IntegrantesEventos> integrantesEventosService,
            IService<Evento> eventoService,
            IService<Usuario> usuarioService)
        {
            _logger = logger;
            IntegrantesEventosService = integrantesEventosService;
            EventoService = eventoService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetIntegrantesEventossForCombo")]
        public async Task<List<IntegrantesEventos>> GetIntegrantesEventossForCombo(string? query = null)
        {
            Expression<Func<IntegrantesEventos, bool>> ex = null;
            Expression<Func<IntegrantesEventos, bool>> i = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(IntegrantesEventos), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<IntegrantesEventos, bool>>)e;
            }

            List<IntegrantesEventos> IntegrantesEventoss = await IntegrantesEventosService.GetForCombo(ex);

            foreach (IntegrantesEventos IntegrantesEventos in IntegrantesEventoss)
            {
                if (IntegrantesEventos.Id_Evento.HasValue)
                {
                    IntegrantesEventos.Evento = await EventoService.GetById(IntegrantesEventos.Id_Evento.Value);
                }

                if (IntegrantesEventos.Id_Usuario.HasValue)
                {
                    IntegrantesEventos.Usuario = await UsuarioService.GetById(IntegrantesEventos.Id_Usuario.Value);
                }
            }

            return IntegrantesEventoss;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<IntegrantesEventos> GetById(int id)
        {
            IntegrantesEventos IntegrantesEventos = await IntegrantesEventosService.GetById(id);

            if (IntegrantesEventos != null)
            {
                if (IntegrantesEventos.Id_Evento.HasValue)
                {
                    IntegrantesEventos.Evento = await EventoService.GetById(IntegrantesEventos.Id_Evento.Value);
                }

                if (IntegrantesEventos.Id_Usuario.HasValue)
                {
                    IntegrantesEventos.Usuario = await UsuarioService.GetById(IntegrantesEventos.Id_Usuario.Value);
                }
            }

            return IntegrantesEventos;
        }

        [HttpPost]
        [Route("CreateIntegrantesEventos")]
        public async Task<IntegrantesEventos> CreateIntegrantesEventos(string integrantesEventoss)
        {
            IntegrantesEventos IntegrantesEventos = JsonConvert.DeserializeObject<IntegrantesEventos>(integrantesEventoss);
            return await IntegrantesEventosService.Create(IntegrantesEventos);
        }

        [HttpPost]
        [Route("UpdateIntegrantesEventos")]
        public async Task<IntegrantesEventos> UpdateIntegrantesEventos(string integrantesEventoss)
        {
            IntegrantesEventos IntegrantesEventos = JsonConvert.DeserializeObject<IntegrantesEventos>(integrantesEventoss);
            return await IntegrantesEventosService.Update(IntegrantesEventos);
        }

        [HttpPost]
        [Route("DeleteIntegrantesEventos")]
        public async void DeleteIntegrantesEventos(string integrantesEventoss)
        {
            IntegrantesEventos IntegrantesEventos = JsonConvert.DeserializeObject<IntegrantesEventos>(integrantesEventoss);
            IntegrantesEventosService.Delete(IntegrantesEventos);
        }
    }
}

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
    public class EventoController : ControllerBase
    {
        private readonly ILogger<EventoController> _logger;
        private readonly IService<Evento> EventoService;

        public EventoController(ILogger<EventoController> logger, IService<Evento> eventoService)
        {
            _logger = logger;
            EventoService = eventoService;
        }

        [HttpGet]
        [Route("GetEventosForCombo")]
        public async Task<List<Evento>> GetEventosForCombo(string? query = null)
        {
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Evento), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                var ex = (Expression<Func<Evento, bool>>)e;
                return await EventoService.GetForCombo(ex);
            }
            else
            {
                return await EventoService.GetForCombo();
            }
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Evento> GetById(int id)
        {
            return await EventoService.GetById(id);
        }

        [HttpPost]
        [Route("CreateEvento")]
        public async Task<Evento> CreateEvento(string eventos)
        {
            Evento evento = JsonConvert.DeserializeObject<Evento>(eventos);
            return await EventoService.Create(evento);
        }

        [HttpPost]
        [Route("UpdateEvento")]
        public async Task<Evento> UpdateEvento(string eventos)
        {
            Evento evento = JsonConvert.DeserializeObject<Evento>(eventos);
            return await EventoService.Update(evento);
        }

        [HttpPost]
        [Route("DeleteEvento")]
        public async void DeleteEvento(string eventos)
        {
            Evento evento = JsonConvert.DeserializeObject<Evento>(eventos);
            EventoService.Delete(evento);
        }
    }
}

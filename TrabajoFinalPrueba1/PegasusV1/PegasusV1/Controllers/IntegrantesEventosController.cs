﻿using Microsoft.AspNetCore.Mvc;
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
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(IntegrantesEventos), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<IntegrantesEventos, bool>>)e;
            }

            List<IntegrantesEventos> IntegrantesEventoss = await IntegrantesEventosService.GetIntegrantesEventosForCombo(ex);

            return IntegrantesEventoss;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<IntegrantesEventos?> GetById(int id)
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
        public async Task<IntegrantesEventos> CreateIntegrantesEventos(IntegrantesEventos integrantesEventos)
        {
            return await IntegrantesEventosService.Create(integrantesEventos);
        }

        [HttpPut]
        [Route("UpdateIntegrantesEventos")]
        public async Task<IntegrantesEventos> UpdateIntegrantesEventos(IntegrantesEventos integrantesEventos)
        {
            return await IntegrantesEventosService.Update(integrantesEventos);
        }

        [HttpGet]
        [Route("DeleteIntegrantesEventos")]
        public async Task DeleteIntegrantesEventos(int id)
        {
            IntegrantesEventos? integrantesEventos = await IntegrantesEventosService.GetById(id);
            if(integrantesEventos != null)
                await IntegrantesEventosService.Delete(integrantesEventos);
        }

        [HttpPut]
        [Route("CreateAllIntegrantesEventos")]
        public async Task<List<IntegrantesEventos>> CreateAllIntegrantesEventos(List<IntegrantesEventos> integrantesEventos)
        {
            return await IntegrantesEventosService.CreateAll(integrantesEventos);
        }

        [HttpPut]
        [Route("UpdateAllIntegrantesEventos")]
        public async Task<List<IntegrantesEventos>> UpdateAllIntegrantesEventos(List<IntegrantesEventos> integrantesEventos)
        {
            return await IntegrantesEventosService.UpdateAll(integrantesEventos);
        }

        [HttpGet]
        [Route("DeleteAllIntegrantesEventos")]
        public async Task DeleteAllIntegrantesEventos(List<IntegrantesEventos> integrantesEventos)
        {
            await IntegrantesEventosService.DeleteAll(integrantesEventos);
        }
    }
}

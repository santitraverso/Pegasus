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
    public class ContenidoMateriasController : ControllerBase
    {
        private readonly ILogger<ContenidoMateriasController> _logger;
        private readonly IService<ContenidoMaterias> ContenidoMateriasService;
        private readonly IService<Materia> MateriaService;

        public ContenidoMateriasController(ILogger<ContenidoMateriasController> logger,
            IService<ContenidoMaterias> contenidoMateriasService,
            IService<Materia> materiaService)
        {
            _logger = logger;
            ContenidoMateriasService = contenidoMateriasService;
            MateriaService = materiaService;
        }

        [HttpGet]
        [Route("GetContenidoMateriasForCombo")]
        public async Task<List<ContenidoMaterias>> GetContenidoMateriasForCombo(string? query = null)
        {
            Expression<Func<ContenidoMaterias, bool>> ex = null;
            Expression<Func<ContenidoMaterias, bool>> i = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(ContenidoMaterias), query);
                var p2 = Expression.Parameter(typeof(Materia), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<ContenidoMaterias, bool>>)e;
            }

            List<ContenidoMaterias> ContenidoMateriass = await ContenidoMateriasService.GetContenidoMateriasForCombo(ex);

            return ContenidoMateriass;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ContenidoMaterias?> GetById(int id)
        {
            ContenidoMaterias ContenidoMaterias = await ContenidoMateriasService.GetById(id);

            if (ContenidoMaterias != null)
            {
                if (ContenidoMaterias.Id_Materia.HasValue)
                {
                    ContenidoMaterias.Materia = await MateriaService.GetById(ContenidoMaterias.Id_Materia.Value);
                }
            }

            return ContenidoMaterias;
        }

        [HttpPost]
        [Route("CreateContenidoMaterias")]
        public async Task<ContenidoMaterias> CreateContenidoMaterias(ContenidoMaterias ContenidoMaterias)
        {
            return await ContenidoMateriasService.Create(ContenidoMaterias);
        }

        [HttpPut]
        [Route("UpdateContenidoMaterias")]
        public async Task<ContenidoMaterias> UpdateContenidoMaterias(ContenidoMaterias ContenidoMaterias)
        {
            return await ContenidoMateriasService.Update(ContenidoMaterias);
        }

        [HttpGet]
        [Route("DeleteContenidoMaterias")]
        public async Task DeleteContenidoMaterias(int id)
        {
            ContenidoMaterias? ContenidoMaterias = await ContenidoMateriasService.GetById(id);
            if (ContenidoMaterias != null)
                await ContenidoMateriasService.Delete(ContenidoMaterias);
        }

        [HttpPut]
        [Route("CreateAllContenidoMaterias")]
        public async Task<List<ContenidoMaterias>> CreateAllContenidoMaterias(List<ContenidoMaterias> ContenidoMaterias)
        {
            return await ContenidoMateriasService.CreateAll(ContenidoMaterias);
        }

        [HttpPut]
        [Route("UpdateAllContenidoMaterias")]
        public async Task<List<ContenidoMaterias>> UpdateAllContenidoMaterias(List<ContenidoMaterias> ContenidoMaterias)
        {
            return await ContenidoMateriasService.UpdateAll(ContenidoMaterias);
        }

        [HttpGet]
        [Route("DeleteAllContenidoMaterias")]
        public async Task DeleteAllContenidoMaterias(List<ContenidoMaterias> ContenidoMaterias)
        {
            await ContenidoMateriasService.DeleteAll(ContenidoMaterias);
        }
    }
}

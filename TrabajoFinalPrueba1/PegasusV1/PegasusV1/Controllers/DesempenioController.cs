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

        public DesempenioController(ILogger<DesempenioController> logger,
            IService<Desempenio> desempenioService)
        {
            _logger = logger;
            DesempenioService = desempenioService;
        }

        [HttpGet]
        [Route("GetDesempenosForCombo")]
        public async Task<List<Desempenio>> GetDesempenosForCombo(string? query = null)
        {
            Expression<Func<Desempenio, bool>> ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Desempenio), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Desempenio, bool>>)e;
            }

            List<Desempenio> Desempenos = await DesempenioService.GetDesempenoForCombo(ex);

            return Desempenos;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Desempenio?> GetById(int id)
        {
            Desempenio Desempeno = await DesempenioService.GetById(id);

            return Desempeno;
        }

        [HttpPost]
        [Route("CreateDesempeno")]
        public async Task<Desempenio> CreateDesempeno(Desempenio Desempeno)
        {
            return await DesempenioService.Create(Desempeno);
        }

        [HttpPut]
        [Route("UpdateDesempeno")]
        public async Task<Desempenio> UpdateDesempeno(Desempenio Desempeno)
        {
            return await DesempenioService.Update(Desempeno);
        }

        [HttpGet]
        [Route("DeleteDesempeno")]
        public async Task DeleteDesempeno(int id)
        {
            Desempenio? Desempeno = await DesempenioService.GetById(id);
            if (Desempeno != null)
                await DesempenioService.Delete(Desempeno);
        }

        [HttpPut]
        [Route("CreateAllDesempeno")]
        public async Task<List<Desempenio>> CreateAllDesempeno(List<Desempenio> Desempenos)
        {
            return await DesempenioService.CreateAll(Desempenos);
        }

        [HttpPut]
        [Route("UpdateAllDesempeno")]
        public async Task<List<Desempenio>> UpdateAllDesempeno(List<Desempenio> Desempenos)
        {
            return await DesempenioService.UpdateAll(Desempenos);
        }

        [HttpGet]
        [Route("DeleteAllDesempeno")]
        public async Task DeleteAllDesempeno(List<Desempenio> Desempenos)
        {
            await DesempenioService.DeleteAll(Desempenos);
        }
    }
}

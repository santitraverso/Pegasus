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
    public class DesempenoController : ControllerBase
    {
        private readonly ILogger<DesempenoController> _logger;
        private readonly IService<Desempeno> DesempenoService;
        private readonly IService<Materia> MateriaService;
        private readonly IService<Usuario> UsuarioService;

        public DesempenoController(ILogger<DesempenoController> logger,
            IService<Desempeno> desempenoService,
            IService<Materia> materiaService)
        {
            _logger = logger;
            DesempenoService = desempenoService;
            MateriaService = materiaService;
        }

        [HttpGet]
        [Route("GetDesempenosForCombo")]
        public async Task<List<Desempeno>> GetDesempenosForCombo(string? query = null)
        {
            Expression<Func<Desempeno, bool>> ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Desempeno), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Desempeno, bool>>)e;
            }

            List<Desempeno> Desempenos = await DesempenoService.GetDesempenoForCombo(ex);

            return Desempenos;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Desempeno?> GetById(int id)
        {
            Desempeno Desempeno = await DesempenoService.GetById(id);

            return Desempeno;
        }

        [HttpPost]
        [Route("CreateDesempeno")]
        public async Task<Desempeno> CreateDesempeno(Desempeno Desempeno)
        {
            return await DesempenoService.Create(Desempeno);
        }

        [HttpPut]
        [Route("UpdateDesempeno")]
        public async Task<Desempeno> UpdateDesempeno(Desempeno Desempeno)
        {
            return await DesempenoService.Update(Desempeno);
        }

        [HttpGet]
        [Route("DeleteDesempeno")]
        public async Task DeleteDesempeno(int id)
        {
            Desempeno? Desempeno = await DesempenoService.GetById(id);
            if (Desempeno != null)
                await DesempenoService.Delete(Desempeno);
        }

        [HttpPut]
        [Route("CreateAllDesempeno")]
        public async Task<List<Desempeno>> CreateAllDesempeno(List<Desempeno> Desempenos)
        {
            return await DesempenoService.CreateAll(Desempenos);
        }

        [HttpPut]
        [Route("UpdateAllDesempeno")]
        public async Task<List<Desempeno>> UpdateAllDesempeno(List<Desempeno> Desempenos)
        {
            return await DesempenoService.UpdateAll(Desempenos);
        }

        [HttpGet]
        [Route("DeleteAllDesempeno")]
        public async Task DeleteAllDesempeno(List<Desempeno> Desempenos)
        {
            await DesempenoService.DeleteAll(Desempenos);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Web;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PerfilesController : ControllerBase
    {
        private readonly ILogger<PerfilesController> _logger;
        private readonly IService<Perfiles> PerfilesService;

        public PerfilesController(ILogger<PerfilesController> logger,
            IService<Perfiles> perfilesService)
        {
            _logger = logger;
            PerfilesService = perfilesService;
        }

        [HttpGet]
        [Route("GetPerfilesForCombo")]
        public async Task<List<Perfiles>> GetPerfilessForCombo(string? query = null)
        {
            Expression<Func<Perfiles, bool>>? ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Perfiles), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Perfiles, bool>>)e;
            }

            List<Perfiles> Perfiless = await PerfilesService.GetPerfilesForCombo(ex);

            return Perfiless;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Perfiles?> GetById(int id)
        {
            Perfiles Perfiles = await PerfilesService.GetById(id);

            return Perfiles;
        }

        [HttpPost]
        [Route("CreatePerfiles")]
        public async Task<Perfiles> CreatePerfiles(Perfiles Perfiles)
        {
            return await PerfilesService.Create(Perfiles);
        }

        [HttpPut]
        [Route("UpdatePerfiles")]
        public async Task<Perfiles> UpdatePerfiles(Perfiles Perfiles)
        {
            return await PerfilesService.Update(Perfiles);
        }

        [HttpGet]
        [Route("DeletePerfiles")]
        public async Task DeletePerfiles(int id)
        {
            Perfiles? Perfiles = await PerfilesService.GetById(id);
            if (Perfiles != null)
                await PerfilesService.Delete(Perfiles);
        }

        [HttpPut]
        [Route("CreateAllPerfiles")]
        public async Task<List<Perfiles>> CreateAllPerfiles(List<Perfiles> Perfiless)
        {
            return await PerfilesService.CreateAll(Perfiless);
        }

        [HttpPut]
        [Route("UpdateAllPerfiles")]
        public async Task<List<Perfiles>> UpdateAllPerfiles(List<Perfiles> Perfiless)
        {
            return await PerfilesService.UpdateAll(Perfiless);
        }

        [HttpGet]
        [Route("DeleteAllPerfiles")]
        public async Task DeleteAllPerfiles(List<Perfiles> Perfiless)
        {
            await PerfilesService.DeleteAll(Perfiless);
        }

    }
}

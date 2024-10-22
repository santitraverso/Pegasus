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
    public class ModulosController : ControllerBase
    {
        private readonly ILogger<ModulosController> _logger;
        private readonly IService<Modulos> ModulosService;

        public ModulosController(ILogger<ModulosController> logger,
            IService<Modulos> modulosService)
        {
            _logger = logger;
            ModulosService = modulosService;
        }

        [HttpGet]
        [Route("GetModulosForCombo")]
        public async Task<List<Modulos>> GetModulossForCombo(string? query = null)
        {
            Expression<Func<Modulos, bool>>? ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Modulos), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Modulos, bool>>)e;
            }

            List<Modulos> Moduloss = await ModulosService.GetModulosForCombo(ex);

            return Moduloss;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Modulos?> GetById(int id)
        {
            return await ModulosService.GetById(id);
        }

        [HttpPost]
        [Route("CreateModulos")]
        public async Task<Modulos> CreateModulos(Modulos Modulos)
        {
            return await ModulosService.Create(Modulos);
        }

        [HttpPut]
        [Route("UpdateModulos")]
        public async Task<Modulos> UpdateModulos(Modulos Modulos)
        {
            return await ModulosService.Update(Modulos);
        }

        [HttpGet]
        [Route("DeleteModulos")]
        public async Task DeleteModulos(int id)
        {
            Modulos? Modulos = await ModulosService.GetById(id);
            if (Modulos != null)
                await ModulosService.Delete(Modulos);
        }

        [HttpPut]
        [Route("CreateAllModulos")]
        public async Task<List<Modulos>> CreateAllModulos(List<Modulos> Moduloss)
        {
            return await ModulosService.CreateAll(Moduloss);
        }

        [HttpPut]
        [Route("UpdateAllModulos")]
        public async Task<List<Modulos>> UpdateAllModulos(List<Modulos> Moduloss)
        {
            return await ModulosService.UpdateAll(Moduloss);
        }

        [HttpGet]
        [Route("DeleteAllModulos")]
        public async Task DeleteAllModulos(List<Modulos> Moduloss)
        {
            await ModulosService.DeleteAll(Moduloss);
        }

    }
}

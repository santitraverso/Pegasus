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
    public class MateriaController : ControllerBase
    {
        private readonly ILogger<MateriaController> _logger;
        private readonly IService<Materia> MateriaService;

        public MateriaController(ILogger<MateriaController> logger, IService<Materia> materiaService)
        {
            _logger = logger;
            MateriaService = materiaService;
        }

        [HttpGet]
        [Route("GetMateriasForCombo")]
        public async Task<List<Materia>> GetMateriasForCombo(string? query = null)
        {
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Materia), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                var ex = (Expression<Func<Materia, bool>>)e;
                return await MateriaService.GetForCombo(ex);
            }
            else
            {
                return await MateriaService.GetForCombo();
            }
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Materia?> GetById(int id)
        {
            return await MateriaService.GetById(id);
        }

        [HttpPost]
        [Route("CreateMateria")]
        public async Task<Materia> CreateMateria(Materia materia)
        {
            return await MateriaService.Create(materia);
        }

        [HttpPut]
        [Route("UpdateMateria")]
        public async Task<Materia> UpdateMateria(Materia materia)
        {
            return await MateriaService.Update(materia);
        }

        [HttpDelete]
        [Route("DeleteMateria")]
        public async Task DeleteMateria(int id)
        {
            Materia? materia = await GetById(id);
            if(materia != null)
                await MateriaService.Delete(materia);
        }
    }
}

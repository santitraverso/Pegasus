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
        public async Task<Materia> GetById(int id)
        {
            return await MateriaService.GetById(id);
        }

        [HttpPost]
        [Route("CreateMateria")]
        public async Task<Materia> CreateMateria(string subject)
        {
            Materia materia = JsonConvert.DeserializeObject<Materia>(subject);
            return await MateriaService.Create(materia);
        }

        [HttpPost]
        [Route("UpdateMateria")]
        public async Task<Materia> UpdateMateria(string subject)
        {
            Materia materia = JsonConvert.DeserializeObject<Materia>(subject);
            return await MateriaService.Update(materia);
        }

        [HttpPost]
        [Route("DeleteMateria")]
        public async void DeleteMateria(string subject)
        {
            Materia materia = JsonConvert.DeserializeObject<Materia>(subject);
            MateriaService.Delete(materia);
        }
    }
}

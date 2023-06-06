using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using PegasusV1.Services;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalificacionMateriaController : ControllerBase
    {
        private readonly ILogger<CalificacionMateriaController> _logger;
        private readonly IService<CalificacionMateria> CalificacionMateriaService;

        public CalificacionMateriaController(ILogger<CalificacionMateriaController> logger, IService<CalificacionMateria> calificacionMateriaService)
        {
            _logger = logger;
            CalificacionMateriaService = calificacionMateriaService;
        }

        [HttpGet]
        [Route("GetCalificacionMateriasForCombo")]
        public async Task<List<CalificacionMateria>> GetCalificacionMateriasForCombo(string? query = null)
        {
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(CalificacionMateria), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                var ex = (Expression<Func<CalificacionMateria, bool>>)e;
                return await CalificacionMateriaService.GetForCombo(ex);
            }
            else
            {
                return await CalificacionMateriaService.GetForCombo();
            }
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<CalificacionMateria?> GetById(int id)
        {
            return await CalificacionMateriaService.GetById(id);
        }
    }
}

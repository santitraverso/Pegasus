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
    public class IntegrantesMateriasController : ControllerBase
    {
        private readonly ILogger<IntegrantesMateriasController> _logger;
        private readonly IService<IntegrantesMaterias> IntegrantesMateriasService;
        private readonly IService<Materia> MateriaService;
        private readonly IService<Usuario> UsuarioService;

        public IntegrantesMateriasController(ILogger<IntegrantesMateriasController> logger,
            IService<IntegrantesMaterias> integrantesMateriasService,
            IService<Materia> materiaService,
            IService<Usuario> usuarioService)
        {
            _logger = logger;
            IntegrantesMateriasService = integrantesMateriasService;
            MateriaService = materiaService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetIntegrantesMateriassForCombo")]
        public async Task<List<IntegrantesMaterias>> GetIntegrantesMateriassForCombo(string? query = null)
        {
            Expression<Func<IntegrantesMaterias, bool>> ex = null;
            Expression<Func<IntegrantesMaterias, bool>> i = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(IntegrantesMaterias), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<IntegrantesMaterias, bool>>)e;
            }

            List<IntegrantesMaterias> IntegrantesMateriass = await IntegrantesMateriasService.GetForCombo(ex);

            foreach (IntegrantesMaterias IntegrantesMaterias in IntegrantesMateriass)
            {
                if (IntegrantesMaterias.Id_Materia.HasValue)
                {
                    IntegrantesMaterias.Materia = await MateriaService.GetById(IntegrantesMaterias.Id_Materia.Value);
                }

                if (IntegrantesMaterias.Id_Usuario.HasValue)
                {
                    IntegrantesMaterias.Usuario = await UsuarioService.GetById(IntegrantesMaterias.Id_Usuario.Value);
                }
            }

            return IntegrantesMateriass;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<IntegrantesMaterias> GetById(int id)
        {
            IntegrantesMaterias IntegrantesMaterias = await IntegrantesMateriasService.GetById(id);

            if (IntegrantesMaterias != null)
            {
                if (IntegrantesMaterias.Id_Materia.HasValue)
                {
                    IntegrantesMaterias.Materia = await MateriaService.GetById(IntegrantesMaterias.Id_Materia.Value);
                }

                if (IntegrantesMaterias.Id_Usuario.HasValue)
                {
                    IntegrantesMaterias.Usuario = await UsuarioService.GetById(IntegrantesMaterias.Id_Usuario.Value);
                }
            }

            return IntegrantesMaterias;
        }

        [HttpPost]
        [Route("CreateIntegrantesMaterias")]
        public async Task<IntegrantesMaterias> CreateIntegrantesMaterias(string integrantesMateriass)
        {
            IntegrantesMaterias IntegrantesMaterias = JsonConvert.DeserializeObject<IntegrantesMaterias>(integrantesMateriass);
            return await IntegrantesMateriasService.Create(IntegrantesMaterias);
        }

        [HttpPost]
        [Route("UpdateIntegrantesMaterias")]
        public async Task<IntegrantesMaterias> UpdateIntegrantesMaterias(string integrantesMateriass)
        {
            IntegrantesMaterias IntegrantesMaterias = JsonConvert.DeserializeObject<IntegrantesMaterias>(integrantesMateriass);
            return await IntegrantesMateriasService.Update(IntegrantesMaterias);
        }

        [HttpPost]
        [Route("DeleteIntegrantesMaterias")]
        public async void DeleteIntegrantesMaterias(string integrantesMateriass)
        {
            IntegrantesMaterias IntegrantesMaterias = JsonConvert.DeserializeObject<IntegrantesMaterias>(integrantesMateriass);
            IntegrantesMateriasService.Delete(IntegrantesMaterias);
        }
    }
}

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
        private readonly IService<Calificaciones> CalificacionesService;

        public IntegrantesMateriasController(ILogger<IntegrantesMateriasController> logger,
            IService<IntegrantesMaterias> integrantesMateriasService,
            IService<Materia> materiaService,
            IService<Usuario> usuarioService,
            IService<Calificaciones> calificacionesService)
        {
            _logger = logger;
            IntegrantesMateriasService = integrantesMateriasService;
            MateriaService = materiaService;
            UsuarioService = usuarioService;
            CalificacionesService = calificacionesService;
        }

        [HttpGet]
        [Route("GetIntegrantesMateriasForCombo")]
        public async Task<List<IntegrantesMaterias>> GetIntegrantesMateriasForCombo(string? query = null)
        {
            Expression<Func<IntegrantesMaterias, bool>> ex = null;
            Expression<Func<IntegrantesMaterias, bool>> i = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(IntegrantesMaterias), query);
                var p2 = Expression.Parameter(typeof(Materia), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<IntegrantesMaterias, bool>>)e;
            }

            List<IntegrantesMaterias> IntegrantesMateriass = await IntegrantesMateriasService.GetIntegrantesMateriasForCombo(ex);

            foreach (IntegrantesMaterias intMat in IntegrantesMateriass)
            {
                if (intMat.Id_Usuario.HasValue && intMat.Id_Materia.HasValue)
                {
                    List<Calificaciones> calificaciones = await CalificacionesService.GetCalificacionesByUserAndMateria(intMat.Id_Usuario.Value, intMat.Id_Materia.Value);
                    intMat.Usuario = await UsuarioService.GetById(intMat.Id_Usuario.Value);
                    intMat.Usuario!.Calificaciones = calificaciones;
                }
            }

            return IntegrantesMateriass;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<IntegrantesMaterias?> GetById(int id)
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
                    List<Calificaciones> calificaciones = await CalificacionesService.GetCalificacionesByUserAndMateria(IntegrantesMaterias.Id_Usuario.Value, IntegrantesMaterias.Id);
                    IntegrantesMaterias.Usuario!.Calificaciones = calificaciones;
                }
            }

            return IntegrantesMaterias;
        }

        [HttpPost]
        [Route("CreateIntegrantesMaterias")]
        public async Task<IntegrantesMaterias> CreateIntegrantesMaterias(IntegrantesMaterias integrantesMaterias)
        {
            return await IntegrantesMateriasService.Create(integrantesMaterias);
        }

        [HttpPut]
        [Route("UpdateIntegrantesMaterias")]
        public async Task<IntegrantesMaterias> UpdateIntegrantesMaterias(IntegrantesMaterias integrantesMaterias)
        {
            return await IntegrantesMateriasService.Update(integrantesMaterias);
        }      

        [HttpGet]
        [Route("DeleteIntegrantesMaterias")]
        public async Task DeleteIntegrantesMaterias(int id)
        {
            IntegrantesMaterias? integrantesMaterias = await IntegrantesMateriasService.GetById(id);
            if(integrantesMaterias != null)
                await IntegrantesMateriasService.Delete(integrantesMaterias);
        }

        [HttpPut]
        [Route("CreateAllIntegrantesMaterias")]
        public async Task<List<IntegrantesMaterias>> CreateAllIntegrantesMaterias(List<IntegrantesMaterias> integrantesMaterias)
        {
            return await IntegrantesMateriasService.CreateAll(integrantesMaterias);
        }

        [HttpPut]
        [Route("UpdateAllIntegrantesMaterias")]
        public async Task<List<IntegrantesMaterias>> UpdateAllIntegrantesMaterias(List<IntegrantesMaterias> integrantesMaterias)
        {
            return await IntegrantesMateriasService.UpdateAll(integrantesMaterias);
        }

        [HttpGet]
        [Route("DeleteAllIntegrantesMaterias")]
        public async Task DeleteAllIntegrantesMaterias(List<IntegrantesMaterias> integrantesMaterias)
        {
            await IntegrantesMateriasService.DeleteAll(integrantesMaterias);
        }
    }
}

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
    public class CursoMateriaController : ControllerBase
    {
        private readonly ILogger<CursoMateriaController> _logger;
        private readonly IService<CursoMateria> CursoMateriaService;
        private readonly IService<Materia> MateriaService;
        private readonly IService<Usuario> UsuarioService;
        private readonly IService<Curso> CursoService;
        private readonly IService<Calificaciones> CalificacionesService;

        public CursoMateriaController(ILogger<CursoMateriaController> logger,
            IService<CursoMateria> cursoMateriaService,
            IService<Materia> materiaService,
            IService<Usuario> usuarioService,
            IService<Curso> cursoService,
            IService<Calificaciones> calificacionesService)
        {
            _logger = logger;
            CursoMateriaService = cursoMateriaService;
            MateriaService = materiaService;
            UsuarioService = usuarioService;
            CursoService = cursoService;
            CalificacionesService = calificacionesService;
        }

        [HttpGet]
        [Route("GetCursoMateriaForCombo")]
        public async Task<List<CursoMateria>> GetCursoMateriaForCombo(string? query = null)
        {
            Expression<Func<CursoMateria, bool>> ex = null;
            Expression<Func<CursoMateria, bool>> i = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(CursoMateria), query);
                var p2 = Expression.Parameter(typeof(Materia), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<CursoMateria, bool>>)e;
            }

            List<CursoMateria> CursoMaterias = await CursoMateriaService.GetCursoMateriaForCombo(ex);

            foreach (CursoMateria intMat in CursoMaterias)
            {
                if (intMat.Id_Curso.HasValue && intMat.Id_Materia.HasValue)
                {
                    intMat.Curso = await CursoService.GetById(intMat.Id_Curso.Value);
                }
            }

            return CursoMaterias;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<CursoMateria?> GetById(int id)
        {
            CursoMateria CursoMaterias = await CursoMateriaService.GetById(id);

            if (CursoMaterias != null)
            {
                if (CursoMaterias.Id_Materia.HasValue)
                {
                    CursoMaterias.Materia = await MateriaService.GetById(CursoMaterias.Id_Materia.Value);
                }

                if (CursoMaterias.Id_Curso.HasValue)
                {
                    CursoMaterias.Curso = await CursoService.GetById(CursoMaterias.Id_Curso.Value);
                }
            }

            return CursoMaterias;
        }

        [HttpPost]
        [Route("CreateCursoMateria")]
        public async Task<CursoMateria> CreateCursoMateria(CursoMateria CursoMateria)
        {
            return await CursoMateriaService.Create(CursoMateria);
        }

        [HttpPut]
        [Route("UpdateCursoMateria")]
        public async Task<CursoMateria> UpdateCursoMateria(CursoMateria CursoMateria)
        {
            return await CursoMateriaService.Update(CursoMateria);
        }      

        [HttpGet]
        [Route("DeleteCursoMateria")]
        public async Task DeleteCursoMateria(int id)
        {
            CursoMateria? CursoMateria = await CursoMateriaService.GetById(id);
            if(CursoMateria != null)
                await CursoMateriaService.Delete(CursoMateria);
        }

        [HttpPut]
        [Route("CreateAllCursoMateria")]
        public async Task<List<CursoMateria>> CreateAllCursoMateria(List<CursoMateria> CursoMateria)
        {
            return await CursoMateriaService.CreateAll(CursoMateria);
        }

        [HttpPut]
        [Route("UpdateAllCursoMateria")]
        public async Task<List<CursoMateria>> UpdateAllCursoMateria(List<CursoMateria> CursoMateria)
        {
            return await CursoMateriaService.UpdateAll(CursoMateria);
        }

        [HttpGet]
        [Route("DeleteAllCursoMateria")]
        public async Task DeleteAllCursoMateria(List<CursoMateria> CursoMateria)
        {
            await CursoMateriaService.DeleteAll(CursoMateria);
        }
    }
}

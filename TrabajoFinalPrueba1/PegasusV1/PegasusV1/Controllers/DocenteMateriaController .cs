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
    public class DocenteMateriaController : ControllerBase
    {
        private readonly ILogger<DocenteMateriaController> _logger;
        private readonly IService<DocenteMateria> DocenteMateriaService;
        private readonly IService<Materia> MateriaService;
        private readonly IService<Usuario> UsuarioService;
        private readonly IService<Curso> CursoService;

        public DocenteMateriaController(ILogger<DocenteMateriaController> logger,
            IService<DocenteMateria> docenteMateriaService,
            IService<Materia> materiaService,
            IService<Usuario> usuarioService,
            IService<Curso> cursoService)
        {
            _logger = logger;
            DocenteMateriaService = docenteMateriaService;
            MateriaService = materiaService;
            UsuarioService = usuarioService;
            CursoService = cursoService;
        }

        [HttpGet]
        [Route("GetDocenteMateriaForCombo")]
        public async Task<List<DocenteMateria>> GetDocenteMateriaForCombo(string? query = null)
        {
            Expression<Func<DocenteMateria, bool>> ex = null;
            Expression<Func<DocenteMateria, bool>> i = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(DocenteMateria), query);
                var p2 = Expression.Parameter(typeof(Materia), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<DocenteMateria, bool>>)e;
            }

            List<DocenteMateria> DocenteMaterias = await DocenteMateriaService.GetDocenteMateriaForCombo(ex);

            foreach (DocenteMateria intMat in DocenteMaterias)
            {
                if (intMat.Id_Curso.HasValue && intMat.Id_Materia.HasValue && intMat.Id_Docente.HasValue)
                {
                    intMat.Curso = await CursoService.GetById(intMat.Id_Curso.Value);

                    intMat.Materia = await MateriaService.GetById(intMat.Id_Materia.Value);

                    intMat.Docente = await UsuarioService.GetById(intMat.Id_Docente.Value);
                }
            }

            return DocenteMaterias;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<DocenteMateria?> GetById(int id)
        {
            DocenteMateria DocenteMaterias = await DocenteMateriaService.GetById(id);

            if (DocenteMaterias != null)
            {
                if (DocenteMaterias.Id_Materia.HasValue)
                {
                    DocenteMaterias.Materia = await MateriaService.GetById(DocenteMaterias.Id_Materia.Value);
                }

                if (DocenteMaterias.Id_Curso.HasValue)
                {
                    DocenteMaterias.Curso = await CursoService.GetById(DocenteMaterias.Id_Curso.Value);
                }

                if (DocenteMaterias.Id_Docente.HasValue)
                {
                    DocenteMaterias.Docente = await UsuarioService.GetById(DocenteMaterias.Id_Docente.Value);
                }
            }

            return DocenteMaterias;
        }

        [HttpPost]
        [Route("CreateDocenteMateria")]
        public async Task<DocenteMateria> CreateDocenteMateria(DocenteMateria DocenteMateria)
        {
            return await DocenteMateriaService.Create(DocenteMateria);
        }

        [HttpPut]
        [Route("UpdateDocenteMateria")]
        public async Task<DocenteMateria> UpdateDocenteMateria(DocenteMateria DocenteMateria)
        {
            return await DocenteMateriaService.Update(DocenteMateria);
        }      

        [HttpGet]
        [Route("DeleteDocenteMateria")]
        public async Task DeleteDocenteMateria(int id)
        {
            DocenteMateria? DocenteMateria = await DocenteMateriaService.GetById(id);
            if(DocenteMateria != null)
                await DocenteMateriaService.Delete(DocenteMateria);
        }

        [HttpPut]
        [Route("CreateAllDocenteMateria")]
        public async Task<List<DocenteMateria>> CreateAllDocenteMateria(List<DocenteMateria> DocenteMateria)
        {
            return await DocenteMateriaService.CreateAll(DocenteMateria);
        }

        [HttpPut]
        [Route("UpdateAllDocenteMateria")]
        public async Task<List<DocenteMateria>> UpdateAllDocenteMateria(List<DocenteMateria> DocenteMateria)
        {
            return await DocenteMateriaService.UpdateAll(DocenteMateria);
        }

        [HttpGet]
        [Route("DeleteAllDocenteMateria")]
        public async Task DeleteAllDocenteMateria(List<DocenteMateria> DocenteMateria)
        {
            await DocenteMateriaService.DeleteAll(DocenteMateria);
        }
    }
}

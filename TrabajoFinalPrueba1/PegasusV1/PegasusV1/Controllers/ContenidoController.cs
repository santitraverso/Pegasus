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
    public class ContenidoController : ControllerBase
    {
        private readonly ILogger<ContenidoController> _logger;
        private readonly IService<Contenido> ContenidoService;
        private readonly IService<Materia> MateriaService;
        private readonly IService<Usuario> UsuarioService;

        public ContenidoController(ILogger<ContenidoController> logger,
            IService<Contenido> contenidoService,
            IService<Materia> materiaService)
        {
            _logger = logger;
            ContenidoService = contenidoService;
            MateriaService = materiaService;
        }

        [HttpGet]
        [Route("GetContenidosForCombo")]
        public async Task<List<Contenido>> GetContenidosForCombo(string? query = null)
        {
            Expression<Func<Contenido, bool>> ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Contenido), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Contenido, bool>>)e;
            }

            List<Contenido> Contenidos = await ContenidoService.GetContenidoForCombo(ex);

            return Contenidos;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Contenido?> GetById(int id)
        {
            Contenido Contenido = await ContenidoService.GetById(id);

            if (Contenido != null)
            {
                if (Contenido.Id_Materia.HasValue)
                {
                    Contenido.Materia = await MateriaService.GetById(Contenido.Id_Materia.Value);
                }
            }

            return Contenido;
        }

        [HttpPost]
        [Route("CreateContenido")]
        public async Task<Contenido> CreateContenido(Contenido Contenido)
        {
            return await ContenidoService.Create(Contenido);
        }

        [HttpPut]
        [Route("UpdateContenido")]
        public async Task<Contenido> UpdateContenido(Contenido Contenido)
        {
            return await ContenidoService.Update(Contenido);
        }

        [HttpGet]
        [Route("DeleteContenido")]
        public async Task DeleteContenido(int id)
        {
            Contenido? Contenido = await ContenidoService.GetById(id);
            if (Contenido != null)
                await ContenidoService.Delete(Contenido);
        }

        [HttpPut]
        [Route("CreateAllContenido")]
        public async Task<List<Contenido>> CreateAllContenido(List<Contenido> Contenidos)
        {
            return await ContenidoService.CreateAll(Contenidos);
        }

        [HttpPut]
        [Route("UpdateAllContenido")]
        public async Task<List<Contenido>> UpdateAllContenido(List<Contenido> Contenidos)
        {
            return await ContenidoService.UpdateAll(Contenidos);
        }

        [HttpGet]
        [Route("DeleteAllContenido")]
        public async Task DeleteAllContenido(List<Contenido> Contenidos)
        {
            await ContenidoService.DeleteAll(Contenidos);
        }
    }
}

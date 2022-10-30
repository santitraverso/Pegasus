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
    public class HijoController : ControllerBase
    {
        private readonly ILogger<HijoController> _logger;
        private readonly IService<Hijo> HijoService;
        private readonly IService<Usuario> UsuarioService;

        public HijoController(ILogger<HijoController> logger,
            IService<Hijo> hijoService,
            IService<Usuario> usuarioService)
        {
            _logger = logger;
            HijoService = hijoService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetHijosForCombo")]
        public async Task<List<Hijo>> GetHijosForCombo(string? query = null)
        {
            Expression<Func<Hijo, bool>> ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Hijo), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Hijo, bool>>)e;
            }

            List<Hijo> Hijos = await HijoService.GetHijoForCombo(ex);

            return Hijos;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Hijo?> GetById(int id)
        {
            Hijo Hijo = await HijoService.GetById(id);

            if (Hijo != null)
            {
                if (Hijo.Id_Hijo.HasValue)
                {
                    Hijo.HijoUsuario = await UsuarioService.GetById(Hijo.Id_Hijo.Value);
                }

                if (Hijo.Id_Padre.HasValue)
                {
                    Hijo.Padre = await UsuarioService.GetById(Hijo.Id_Padre.Value);
                }
            }

            return Hijo;
        }

        [HttpPost]
        [Route("CreateHijo")]
        public async Task<Hijo> CreateHijo(Hijo hijo)
        {
            return await HijoService.Create(hijo);
        }

        [HttpPut]
        [Route("UpdateHijo")]
        public async Task<Hijo> UpdateHijo(Hijo hijo)
        {
            return await HijoService.Update(hijo);
        }

        [HttpGet]
        [Route("DeleteHijo")]
        public async Task DeleteHijo(int id)
        {
            Hijo? hijo = await HijoService.GetById(id);
            if(hijo != null)
                await HijoService.Delete(hijo);
        }
    }
}

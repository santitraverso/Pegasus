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
    public class UsuarioController : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger;
        private readonly IService<Usuario> UsuarioService;

        public UsuarioController(ILogger<UsuarioController> logger, IService<Usuario> usuarioService)
        {
            _logger = logger;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetUsuariosForCombo")]
        public async Task<List<Usuario>> GetUsuariosForCombo(string? query = null)
        {
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Usuario), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                var ex = (Expression<Func<Usuario, bool>>)e;
                return await UsuarioService.GetForCombo(ex);
            }
            else
            {
                return await UsuarioService.GetForCombo();
            }
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Usuario?> GetById(int id)
        {
            return await UsuarioService.GetById(id);
        }

        [HttpPost]
        [Route("CreateUsuario")]
        public async Task<Usuario> CreateUsuario(Usuario usuario)
        {
            return await UsuarioService.Create(usuario);
        }

        [HttpPut]
        [Route("UpdateUsuario")]
        public async Task<Usuario> UpdateUsuario(Usuario usuario)
        {
            return await UsuarioService.Update(usuario);
        }

        [HttpDelete]
        [Route("DeleteUsuario")]
        public async Task DeleteUsuario(int id)
        {
            Usuario? usuario = await GetById(id);
            if(usuario != null)
                await UsuarioService.Delete(usuario);
        }
    }
}

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
        public async Task<Usuario> GetById(int id)
        {
            return await UsuarioService.GetById(id);
        }

        [HttpPost]
        [Route("CreateUsuario")]
        public async Task<Usuario> CreateUsuario(string user)
        {
            Usuario usuario = JsonConvert.DeserializeObject<Usuario>(user);
            return await UsuarioService.Create(usuario);
        }

        [HttpPost]
        [Route("UpdateUsuario")]
        public async Task<Usuario> UpdateUsuario(string user)
        {
            Usuario usuario = JsonConvert.DeserializeObject<Usuario>(user);
            return await UsuarioService.Update(usuario);
        }

        [HttpPost]
        [Route("DeleteUsuario")]
        public async void DeleteUsuario(string user)
        {
            Usuario usuario = JsonConvert.DeserializeObject<Usuario>(user);
            UsuarioService.Delete(usuario);
        }
    }
}

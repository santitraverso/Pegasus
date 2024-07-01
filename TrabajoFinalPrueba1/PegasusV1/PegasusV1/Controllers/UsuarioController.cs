using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger;
        private readonly IService<Usuario> UsuarioService;
        private readonly IService<Roles> RolesService;

        public UsuarioController(ILogger<UsuarioController> logger, IService<Usuario> usuarioService,
            IService<Roles> rolesService)
        {
            _logger = logger;
            UsuarioService = usuarioService;
            RolesService = rolesService;
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
            Usuario user = await UsuarioService.GetById(id);

            if (user != null)
            {
                if (user.Perfil.HasValue)
                {
                    user.Rol = await RolesService.GetById(user.Perfil.Value);
                }

            }

            return user;
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

        [HttpGet]
        [Route("DeleteUsuario")]
        public async Task DeleteUsuario(int id)
        {
            Usuario? usuario = await UsuarioService.GetById(id);
            if(usuario != null)
                await UsuarioService.Delete(usuario);
        }

        [HttpPut]
        [Route("CreateAllUsuario")]
        public async Task<List<Usuario>> CreateAllUsuario(List<Usuario> usuarios)
        {
            return await UsuarioService.CreateAll(usuarios);
        }

        [HttpPut]
        [Route("UpdateAllUsuario")]
        public async Task<List<Usuario>> UpdateAllUsuario(List<Usuario> usuarios)
        {
            return await UsuarioService.UpdateAll(usuarios);
        }

        [HttpGet]
        [Route("DeleteAllUsuario")]
        public async Task TaskDeleteAllUsuario(List<Usuario> usuarios)
        {
            await UsuarioService.DeleteAll(usuarios);
        }
    }
}
